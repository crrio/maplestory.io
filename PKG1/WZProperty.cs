using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ImageSharp;

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
         : this(val, original.Name, original.Path, original.FileContainer, original.Type, original.Parent, original.Size, original.Checksum, original.Offset) {

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
         : this(val, original.Name, original.Path, original.FileContainer, original.Type, original.Parent, original.Size, original.Checksum, original.Offset) { _weakChildren = original._weakChildren; }
        public WZPropertyVal(K val, string name, string path, Package container, PropertyType type, WZProperty parent, uint size, int checksum, uint offset) : base(name, path, container, type, parent, size, checksum, offset) {
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
    }
    public class WZProperty {
        public string Name, Path;
        public Package FileContainer;
        public WZProperty Container;
        public PropertyType Type;
        public WZProperty Parent;
        public Dictionary<string, WZProperty> Children{
            get => _children ?? _weakChildren.GetValue();
            set {
                _children = value;
                if(_weakChildren != null){
                    _weakChildren.Dispose();
                    _weakChildren = null;
                }
            }
        }
        Dictionary<string, WZProperty> _children;
        public event Func<Dictionary<string, WZProperty>> LoadChildren;
        internal WeakishReference<Dictionary<string, WZProperty>> _weakChildren;
        public uint Size;
        public int Checksum;
        public uint Offset;

        public uint ContainerStartLocation;
        ~WZProperty() {
            _weakChildren?.Dispose();
        }
        public WZProperty(string name, string path, Package container, PropertyType type, WZProperty parent, uint size, int checksum, uint offset) {
            this._weakChildren = new WeakishReference<Dictionary<string, WZProperty>>(null, () => {
                return LoadChildren != null ? LoadChildren() : null;
            });
            this.Name = name.Replace(".img", "");
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

        public override string ToString() => $"{Path} @ {Offset}x{Size}";

        public WZProperty Resolve(string v = null)
        {
            if (string.IsNullOrEmpty(v) && Type == PropertyType.UOL) {
                IWZPropertyVal<string> self = (IWZPropertyVal<string>)this;
                WZProperty current = this;
                List<string> paths = new List<string>();
                while((current = Parent.Resolve(self.Value)).Type == PropertyType.UOL) {
                    if(paths.Contains(current.Path)) return current;
                    else paths.Add(current.Path);
                }
                return current;
            }

            if (string.IsNullOrEmpty(v)) return this;

            int firstSlash = v.IndexOf('/');
            if (firstSlash == -1) firstSlash = v.Length;

            string childName = v.Substring(0, firstSlash).Replace(".img", "");

            // Dots = children of parent
            if (childName == ".." || childName == ".") return Parent.Resolve(v.Substring(Math.Min(firstSlash + 1, v.Length)));

            if (Children.ContainsKey(childName)) return Children[childName].Resolve(v.Substring(Math.Min(firstSlash + 1, v.Length)));
            else return null;
        }
        public WZProperty ResolveInlink(string v = null)
            =>  Container.Resolve(v);
        public WZProperty ResolveOutlink(string v = null)
            =>  FileContainer.Collection.Resolve(v);

        public K Resolve<K>(string v = null)
            where K : WZProperty
            => AsType<K>(Resolve(v));
        public K ResolveInlink<K>(string v = null)
            where K : WZProperty
            => AsType<K>(ResolveInlink(v));
        public K ResolveOutlink<K>(string v = null)
            where K : WZProperty
            => AsType<K>(ResolveOutlink(v));

        public Nullable<K> ResolveFor<K>(string v = null)
            where K : struct
            => AsType<K>(Resolve(v))?.Value;
        public Nullable<K> ResolveInlinkFor<K>(string v = null)
            where K : struct
            => AsType<K>(ResolveInlink(v))?.Value;
        public Nullable<K> ResolveOutlinkFor<K>(string v = null)
            where K : struct
            => AsType<K>(ResolveOutlink(v))?.Value;
        public K ResolveForOrNull<K>(string v = null)
            where K : class
        {
            WZPropertyVal<K> ret = AsType<K>(Resolve(v));

            if (ret == null) return null;
            // Only follow inlinks and outlinks for canvas elements. Ignore others as we don't know what the side effects would be.
            if (ret?.Type == PropertyType.Canvas) {
                if (ret.Children.ContainsKey("_inlink")) return ResolveInlinkForOrNull<K>(ret.ResolveForOrNull<string>("_inlink"));
                if (ret.Children.ContainsKey("_outlink")) return ResolveOutlinkForOrNull<K>(ret.ResolveForOrNull<string>("_outlink"));
            }
            return ret;
        }
        public K ResolveInlinkForOrNull<K>(string v = null)
            where K : class
            => AsType<K>(ResolveInlink(v))?.Value;
        public K ResolveOutlinkForOrNull<K>(string v = null)
            where K : class
            => AsType<K>(ResolveOutlink(v))?.Value;

        internal static WZPropertyVal<K> AsType<K>(WZProperty prop)
        {
            if (prop == null) return null;

            Type propType = prop.GetType();
            if (propType.Equals(typeof(WZProperty))) return null; //throw new InvalidCastException("Prop is defined as a plain WZProperty and can not be cast as it has no value");

            // If we're asking for the same type as it already is, return as is
            if (typeof(WZPropertyVal<K>).Equals(propType)) return (WZPropertyVal<K>)prop;

            Type convertTo = typeof(K);
            Type convertFrom = propType.GenericTypeArguments[0];

            object currentValue = ((IWZPropertyVal)prop).GetValue();

            if (convertFrom == convertTo) return new WZPropertyVal<K>((K)currentValue, prop);

            if (currentValue is string && convertTo != typeof(string) && convertFrom != convertTo)
                currentValue = decimal.Parse(currentValue.ToString());

            if (currentValue is IConvertible)
                return new WZPropertyVal<K>((K)Convert.ChangeType(
                    currentValue,
                    convertTo
                ), prop);

            return null;
        }
    }

    public enum Region {
        GMS = 0,
        JMS = 1,
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
        File
    }
}