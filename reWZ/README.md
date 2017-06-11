# reWZ
...is a simple and clean .NET library for _reading_ MapleStory WZ files. It does not support writing or modifying WZ files. 

Written with simplicity in mind, it might be slower than other libraries, but it will always be easier to use.

##Usage
Simply reference reWZ.dll in your code, and then load a WZ file like this:

    WZFile xz = new WZFile(@"D:\Effect.wz", WZVariant.GMS, true);

You can resolve a path like so:

    Image<Rgba32> m = xz.ResolvePath("BasicEff.img/LevelUp/7").ValueOrDie<Image<Rgba32>>();

Or, if you prefer to go the old-fashioned way:

    WZCanvasProperty wzcp = (WZCanvasProperty)xz.MainDirectory["BasicEff.img"]["LevelUp"]["7"];
    Image<Rgba32> m = wzcp.Value;

Otherwise, reWZ is pretty well documented (via inline XMLdoc), but if you have any questions, feel free to send angelsl a message.

If you are dumping or otherwise reading the entire WZ file, it is recommended that you read the entire file into memory before parsing, or the parser could take a very long time.

Please do not try to parse entire WZ files into memory with eager canvas and MP3 loading. Even though reWZ allows that, you will need _a lot_ of memory.

Try to run your application in 32-bit mode unless you _really_ need 64-bit. 32-bit is faster and will nearly halve the memory usage.

##License
reWZ is licensed under the GNU GPL v3.0 with Classpath Exception.

##Acknowledgements

 * jonyleeson, haha01haha01, Snow, and others who contributed to the original C# WzLib, and [later versions of it](http://code.google.com/p/maplelib2/).
 * [Fiel](http://www.southperry.net/member.php?u=1) from [Southperry](http://www.southperry.net), for giving me a reason to write this. And all his help.
 * [retep998](https://github.com/retep998), for continually boasting how [his library written in C++](https://github.com/NoLifeDev/NoLifeWz) parses the entire v40b Data.wz in 7 seconds, with canvas properties.
 
##Disacknowledgements
 
 * [zlib](http://www.zlib.net/), for a.. _nice_ library.