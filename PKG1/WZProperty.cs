using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using SixLabors.ImageSharp;
using System.Linq;
using System.Threading.Tasks;

namespace PKG1 {
    public class WZPropertyWeak<K> : WZProperty, IWZPropertyVal<K>
        where K : class
    {
        ~WZPropertyWeak() {
            _weakValue.Dispose();
        }
        WeakishReference<K> _weakValue;
        public K Value {
            get {
                return _weakValue.GetValue();
            }
            set { throw new InvalidOperationException("WeakReference WZProperties can not have strong reference values set."); }
        }
        public WZPropertyWeak(Func<K> val, WZProperty original)
         : this(val, original.NameWithoutExtension, original.Path, original.FileContainer, original.Type, original.Parent, original.Size, original.Checksum, original.Offset) {
            this.Children = original.Children;
            this.Encrypted = original.Encrypted;
        }
        public WZPropertyWeak(Func<K> val, string name, string path, Package container, PropertyType type, WZProperty parent, uint size, int checksum, uint offset) : base(name, path, container, type, parent, size, checksum, offset) {
            _weakValue = new WeakishReference<K>(null, val);
        }
        public object GetValue() => Value;

        public static implicit operator K(WZPropertyWeak<K> prop) => prop.Value;
    }
    public class WZPropertyVal<K> : WZProperty, IWZPropertyVal<K>
    {
        public K Value { get; set; }
        public WZPropertyVal(K val, WZProperty original)
         : this(val, original.NameWithoutExtension, original.Path, original.FileContainer, original.Type, original.Parent, original.Size, original.Checksum, original.Offset) {
            this.Encrypted = original.Encrypted;
        }
        public WZPropertyVal(K val, string name, string path, Package container, PropertyType type, WZProperty parent, uint size, int checksum, uint offset) 
            : base(name, path, container, type, parent, size, checksum, offset) {
            Value = val;
        }
        public static implicit operator K(WZPropertyVal<K> prop) => prop.Value;
        public object GetValue() => Value;
    }
    public interface IWZPropertyVal<K> : IWZPropertyVal {
        K Value { get; set; }
    }
    public interface IWZPropertyVal {
        object GetValue();
        PropertyType Type { get; }
        IEnumerable<WZProperty> Children { get; }
    }
    public class WZProperty {
        public static Func<IEnumerable<WZProperty>, IEnumerable<WZProperty>> ChildrenMutate;
        public static Dictionary<string, Func<WZProperty, string, WZProperty>> SpecialUOL = new Dictionary<string, Func<WZProperty, string, WZProperty>>();

        public string Name, NameWithoutExtension, Path;
        public Package FileContainer;
        public WZProperty Container;
        public PropertyType Type { get; }
        public WZProperty Parent;
        public IEnumerable<WZProperty> Children
        {
            get
            {
                if (_children != null) return _children;
                if (_weakChildren == null) this._weakChildren = new WeakishReference<IEnumerable<WZProperty>>(null, LoadChildren);
                return _weakChildren.GetValue();
            }
            set
            {
                _children = value;
                if (_weakChildren != null)
                {
                    _weakChildren.Dispose();
                    _weakChildren = null;
                }
            }
        }

        Dictionary<string, object> _meta;
        public Dictionary<string, object> Meta
        {
            get
            {
                if (_meta == null) return _meta = new Dictionary<string, object>();
                return _meta;
            }
        }
        public bool HasDefinedMeta
        {
            get => _meta?.Count > 0;
        }

        IEnumerable<WZProperty> _children;
        internal WeakishReference<IEnumerable<WZProperty>> _weakChildren;
        public uint Size;
        public int Checksum;
        public uint Offset;
        public EncryptionType Encrypted;
        public uint ContainerStartLocation;
        ~WZProperty() {
            _weakChildren?.Dispose();
        }
        public WZProperty(string name, string path, Package container, PropertyType type, WZProperty parent, uint size, int checksum, uint offset) {
            this.Name = name;
            this.NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(name);
            this.Path = path;
            this.FileContainer = container;
            this.Parent = parent;
            if (Parent != null) {
                ContainerStartLocation = Parent.ContainerStartLocation;
                if (Parent.Type == PropertyType.Image)
                    Container = Parent;
                else
                    Container = Parent.Container;
            } else {
                ContainerStartLocation = FileContainer.ContentsStartLocation;
                // Assume we are the MainDirectory
                Container = this;
            }
            this.Type = type;
            this.Size = size;
            this.Checksum = checksum;
            this.Offset = offset;
        }

        Task<IEnumerable<WZProperty>> LoadChildrenAsync() => Task<IEnumerable<WZProperty>>.Run<IEnumerable<WZProperty>>(new Func<IEnumerable<WZProperty>>(LoadChildren));
        IEnumerable<WZProperty> LoadChildren()
        {
            IEnumerable<WZProperty> childrenEnumerable = null;
            switch(Type)
            {
                case PropertyType.Image:
                    childrenEnumerable = PropertyResolvers.ImageChildren(this).ToArray();
                    break;
                case PropertyType.Directory:
                    childrenEnumerable = PropertyResolvers.DirectoryChildren(this).ToArray();
                    break;
                case PropertyType.SubProperty:
                    childrenEnumerable = PropertyResolvers.SubPropChildren(this).ToArray();
                    break;
                case PropertyType.Convex:
                    childrenEnumerable = PropertyResolvers.ConvexChildren(this).ToArray();
                    break;
                default:
                    childrenEnumerable = new WZProperty[0];
                    break;
            }

            if (ChildrenMutate != null) return ChildrenMutate(childrenEnumerable);
            else return childrenEnumerable;
        }

        public virtual IEnumerable<WZProperty> GetChildren() => _children ?? LoadChildren();

        public override string ToString() => $"{Path} @ {Offset}x{Size}";

        public Task<WZProperty> ResolveAsync(string v = null) => Task<WZProperty>.Run(() => Resolve(v));
        public WZProperty Resolve(string v = null)
        {
            if (string.IsNullOrEmpty(v) && Type == PropertyType.UOL) {
                IWZPropertyVal<string> self = (IWZPropertyVal<string>)this;
                WZProperty current = this;
                List<string> paths = new List<string>();
                while((current = Parent.Resolve(self.Value))?.Type == PropertyType.UOL) {
                    if(paths.Contains(current.Path)) return current;
                    else paths.Add(current.Path);
                }
                return current;
            }

            if (this.Type == PropertyType.Lua)
            {
                string value = ((IWZPropertyVal<string>)this).Value;
                if (value != null && value.StartsWith("!"))
                {
                    string specialType = value.Substring(1, value.IndexOf(':') - 1);
                    if (SpecialUOL.ContainsKey(specialType))
                    {
                        if (string.IsNullOrEmpty(v)) return SpecialUOL[specialType](this, value.Substring(specialType.Length + 2));
                        else return SpecialUOL[specialType](this, value.Substring(specialType.Length + 2)).Resolve(v);
                    }
                    else throw new InvalidOperationException($"Unable to follow Special UOL, as there is no defined route, Type: {specialType} UOL:{value}");
                }
            }

            if (string.IsNullOrEmpty(v)) return this;

            int forwardSlashPosition = v.IndexOf('/');
            int backSlashPosition = v.IndexOf('\\', 0, forwardSlashPosition == -1 ? v.Length : forwardSlashPosition);
            int firstSlash = -1;

            if (forwardSlashPosition == -1) firstSlash = backSlashPosition;
            else if (backSlashPosition == -1) firstSlash = forwardSlashPosition;
            else firstSlash = Math.Min(forwardSlashPosition, backSlashPosition);

            if (firstSlash == -1) firstSlash = v.Length;

            string childName = v.Substring(0, firstSlash).Replace(".img", "");

            // Dots = children of parent
            if (childName == ".." || childName == ".") return Parent.Resolve(v.Substring(Math.Min(firstSlash + 1, v.Length)));

            WZProperty childMatch = Children.FirstOrDefault(c => c.NameWithoutExtension.Equals(childName, StringComparison.CurrentCultureIgnoreCase));
            if (childMatch != null) {
                if (childMatch.Type == PropertyType.Lua) childMatch = childMatch.Resolve();
                return childMatch.Resolve(v.Substring(Math.Min(firstSlash + 1, v.Length)));
            }

            return childMatch;
        }
        public Task<WZProperty> ResolveInlinkAsync(string v = null) => Task<WZProperty>.Run(() => ResolveInlink(v));
        public WZProperty ResolveInlink(string v = null)
            =>  Container.Resolve(v);
        public Task<WZProperty> ResolveOutlinkAsync(string v = null) => Task<WZProperty>.Run(() => ResolveOutlink(v));
        public WZProperty ResolveOutlink(string v = null)
            =>  FileContainer.Collection.Resolve(v);

        public Task<K> ResolveAsync<K>(string v = null)
            where K : WZProperty
            => Task<K>.Run(() => Resolve<K>(v));
        public Task<K> ResolveInlinkAsync<K>(string v = null)
            where K : WZProperty
            => Task<K>.Run(() => ResolveInlink<K>(v));
        public Task<K> ResolveOutlinkAsync<K>(string v = null)
            where K : WZProperty
            => Task<K>.Run(() => ResolveOutlink<K>(v));

        public K Resolve<K>(string v = null)
            where K : WZProperty
            => (K)AsType<K>(Resolve(v));
        public K ResolveInlink<K>(string v = null)
            where K : WZProperty
            => (K)AsType<K>(ResolveInlink(v));
        public K ResolveOutlink<K>(string v = null)
            where K : WZProperty
            => (K)AsType<K>(ResolveOutlink(v));

        public Task<Nullable<K>> ResolveForAsync<K>(string v = null)
            where K : struct
            => Task<Nullable<K>>.Run(() => ResolveFor<K>(v));
        public Task<Nullable<K>> ResolveInlinkForAsync<K>(string v = null)
            where K : struct
            => Task<Nullable<K>>.Run(() => ResolveInlinkFor<K>(v));
        public Task<Nullable<K>> ResolveOutlinkForAsync<K>(string v = null)
            where K : struct
            => Task<Nullable<K>>.Run(() => ResolveOutlinkFor<K>(v));

        public Nullable<K> ResolveFor<K>(string v = null)
            where K : struct
            => GetValueAs<K>(Resolve(v));
        public Nullable<K> ResolveInlinkFor<K>(string v = null)
            where K : struct
            => GetValueAs<K>(ResolveInlink(v));
        public Nullable<K> ResolveOutlinkFor<K>(string v = null)
            where K : struct
            => GetValueAs<K>(ResolveOutlink(v));

        public Task<K> ResolveForOrNullAsync<K>(string v = null)
            where K : class
            => Task<K>.Run(() => ResolveForOrNull<K>(v));
        public Task<K> ResolveInlinkForOrNullAsync<K>(string v = null)
            where K : class
            => Task<K>.Run(() => ResolveInlinkForOrNull<K>(v));
        public Task<K> ResolveOutlinkForOrNullAsync<K>(string v = null)
            where K : class
            => Task<K>.Run(() => ResolveOutlinkForOrNull<K>(v));

        public K ResolveForOrNull<K>(string v = null)
            where K : class
        {
            IWZPropertyVal<K> ret = AsType<K>(Resolve(v));

            if (ret == null) return null;
            // Only follow inlinks and outlinks for canvas elements. Ignore others as we don't know what the side effects would be.
            if (ret?.Type == PropertyType.Canvas) {

                WZProperty link = ret.Children.FirstOrDefault(c => c.NameWithoutExtension.Equals("_inlink", StringComparison.CurrentCultureIgnoreCase)) ?? ret.Children.FirstOrDefault(c => c.NameWithoutExtension.Equals("_outlink", StringComparison.CurrentCultureIgnoreCase));
                if (link != null)
                {
                    string linksTo = GetValueAsOrNull<string>(link);
                    if (linksTo.StartsWith("Map"))
                    {
                        if (link.Name.Equals("_inlink"))
                            return ResolveInlinkForOrNull<K>(linksTo) ?? ResolveInlinkForOrNull<K>("Map2" + linksTo.Substring(3)) ?? ResolveInlinkForOrNull<K>("Map001" + linksTo.Substring(3));
                        else if (link.Name.Equals("_outlink"))
                            return ResolveOutlinkForOrNull<K>(linksTo) ?? ResolveOutlinkForOrNull<K>("Map2" + linksTo.Substring(3)) ?? ResolveOutlinkForOrNull<K>("Map001" + linksTo.Substring(3));
                    }
                    else if (linksTo.StartsWith("Mob"))
                    {
                        if (link.Name.Equals("_inlink"))
                            return ResolveInlinkForOrNull<K>(linksTo) ?? ResolveInlinkForOrNull<K>("Mob2" + linksTo.Substring(3));
                        else if (link.Name.Equals("_outlink"))
                            return ResolveOutlinkForOrNull<K>(linksTo) ?? ResolveOutlinkForOrNull<K>("Mob2" + linksTo.Substring(3));
                    }
                    else
                    {
                        if (link.Name.Equals("_inlink"))
                            return ResolveInlinkForOrNull<K>(link.ResolveForOrNull<string>());
                        else if (link.Name.Equals("_outlink"))
                            return ResolveOutlinkForOrNull<K>(link.ResolveForOrNull<string>());
                    }
                }
            }

            return ret.Value;
        }
        public K ResolveInlinkForOrNull<K>(string v = null)
            where K : class
            => GetValueAsOrNull<K>(ResolveInlink(v));
        public K ResolveOutlinkForOrNull<K>(string v = null)
            where K : class
            => GetValueAsOrNull<K>(ResolveOutlink(v));

        internal static Nullable<K> GetValueAs<K>(WZProperty prop)
            where K : struct
        {
            if (prop == null) return null;

            Type propType = prop.GetType();
            if (propType.Equals(typeof(WZProperty))) return null;

            // If we're asking for the same type as it already is, return as is
            if (typeof(WZPropertyVal<K>).Equals(propType)) return (WZPropertyVal<K>)prop;

            Type convertTo = typeof(K);
            Type convertFrom = propType.GenericTypeArguments[0];

            IWZPropertyVal propVal = (IWZPropertyVal)prop;
            object currentValue = propVal.GetValue();

            if (convertFrom == convertTo) return (K)currentValue;

            if (currentValue is string && convertTo != typeof(string) && convertFrom != convertTo)
            {
                decimal newValue;
                if (decimal.TryParse(currentValue.ToString(), out newValue))
                    currentValue = newValue;
            }

            if (currentValue is IConvertible)
                try
                {
                    return (K)Convert.ChangeType(
                        currentValue,
                        convertTo
                    );
                }
                catch (Exception ex) { }

            return null;
        }

        internal static K GetValueAsOrNull<K>(WZProperty prop)
            where K : class
        {
            if (prop == null) return null;

            Type propType = prop.GetType();
            if (propType.Equals(typeof(WZProperty))) return null;

            // If we're asking for the same type as it already is, return as is
            if (typeof(WZPropertyVal<K>).Equals(propType)) return (WZPropertyVal<K>)prop;

            Type convertTo = typeof(K);
            Type convertFrom = propType.GenericTypeArguments[0];

            IWZPropertyVal propVal = (IWZPropertyVal)prop;
            object currentValue = propVal.GetValue();

            if (convertFrom == convertTo) return (K)currentValue;

            if (currentValue is string && convertTo != typeof(string) && convertFrom != convertTo)
            {
                decimal newValue;
                if (decimal.TryParse(currentValue.ToString(), out newValue))
                    currentValue = newValue;
            }

            if (currentValue is IConvertible)
                try
                {
                    return (K)Convert.ChangeType(
                        currentValue,
                        convertTo
                    );
                }
                catch (Exception ex) { }

            return null;
        }

        internal static IWZPropertyVal<K> AsType<K>(WZProperty prop)
        {
            if (prop == null) return null;

            Type propType = prop.GetType();
            if (propType.Equals(typeof(WZProperty))) return null;

            // If we're asking for the same type as it already is, return as is
            if (typeof(WZPropertyVal<K>).Equals(propType)) return (WZPropertyVal<K>)prop;
            if (typeof(K).IsClass)
            {
                Type weakType = typeof(WZPropertyWeak<>).MakeGenericType(typeof(K));
                if (weakType.Equals(propType)) return (IWZPropertyVal<K>)prop;
            }

            Type convertTo = typeof(K);
            Type convertFrom = propType.GenericTypeArguments[0];

            IWZPropertyVal propVal = (IWZPropertyVal)prop;
            object currentValue = propVal.GetValue();

            if (convertFrom == convertTo) return new WZPropertyVal<K>((K)currentValue, prop);

            if (currentValue is string && convertTo != typeof(string) && convertFrom != convertTo)
            {
                decimal newValue;
                if (decimal.TryParse(currentValue.ToString(), out newValue))
                    currentValue = newValue;
            }

            if (currentValue is IConvertible)
                try {
                    return new WZPropertyVal<K>((K)Convert.ChangeType(
                        currentValue,
                        convertTo
                    ), prop);
                } catch (Exception ex) { }

            return null;
        }
    }

    public enum Region {
        GMS = 0,
        JMS = 1,
        KMS = 2,
        TMS = 3,
        CMS = 4,
        SEA = 5
    }

    public enum EncryptionType
    {
        None = 0,
        GMS = 1,
        KMS = 2
    }

    public enum PropertyType {
        Directory,
        Image,
        Null,
        UInt16,
        Int32,
        Int64,
        Single,
        Double,
        String,
        Vector2,
        UOL,
        Audio,
        Canvas,
        SubProperty,
        Convex,
        File,
        Lua,
        VersionOutlink // PKG1 / M.IO Specific
    }
}