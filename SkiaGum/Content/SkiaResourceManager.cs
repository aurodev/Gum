﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using SkiaSharp;


namespace SkiaGum.Content
{
    public static class SkiaResourceManager
    {
        public static Assembly CustomResourceAssembly { get; set; }
        static Assembly ResourceAssembly => CustomResourceAssembly ?? Assembly.GetExecutingAssembly();

        #region SVG caching

        private static Dictionary<string, SKSvg> svgCache;

        public static SKSvg GetSvg(string resourceName)
        {
            if (!svgCache.ContainsKey(resourceName))
                CacheSvg(resourceName);

            return svgCache[resourceName];
        }

        private static void CacheSvg(string resourceName)
        {
            using (Stream stream = GetManifestResourceStream(
                resourceName, ResourceAssembly))
            {
                SKSvg svg = new SKSvg();

                try
                {
                    svg.Load(stream);
                }
                catch (Exception e)
                {
                    throw new Exception($"Error loading SVG {resourceName}\n{e}");
                }
                svgCache.Add(resourceName, svg);
            }
        }
        #endregion

        #region SKBitmap caching

        private static Dictionary<string, SKBitmap> skBitmapCache = new Dictionary<string, SKBitmap>();

        public static SKBitmap GetSKBitmap(string resourceName)
        {
            if (!skBitmapCache.ContainsKey(resourceName))
                CacheSKImage(resourceName);

            return skBitmapCache[resourceName];
        }

        private static void CacheSKImage(string resourceName)
        {
            using (var stream = GetManifestResourceStream(
                resourceName, ResourceAssembly))
            {
                var decoded = SKBitmap.Decode(stream);
                skBitmapCache.Add(resourceName, decoded);
            }
        }



        #endregion

        #region Font caching

        private static Dictionary<int, SKTypeface> typefaceCache;

        public enum TypefaceType
        {
            Leelawadee,
            ArialRoundedMTBold,
            RobotoRegular,
            RobotoMedium,
            RobotoMediumItalic,
            RobotoBold
        }

        public static SKTypeface GetTypeface(TypefaceType type) => typefaceCache[(int)type];

        public static SKTypeface GetTypeface(string typefaceName)
        {
            //Do not enclose this stream in a using block: https://stackoverflow.com/questions/48061401/drawtext-in-canvas-skiasharp-text-does-not-display
            Stream stream = GetManifestResourceStream(
                typefaceName, ResourceAssembly);

            var typeface = SKTypeface.FromStream(stream);
            //typefaceCache.Add(type, typeface);
            // todo - cache it:
            return typeface;
        }

        //private static void CacheTypeface(int type)
        //{
        //    SKTypeface result = SKTypeface.Default;
        //    string fullFontName = "Namespace.Resources.Fonts.";

        //    switch (type)
        //    {
        //        case (int)TypefaceType.Leelawadee:
        //            fullFontName += Fonts.CustomFont.Leelawadee.IosName;
        //            break;

        //        case (int)TypefaceType.ArialRoundedMTBold:
        //            fullFontName += Fonts.CustomFont.ArialRoundedMTBold.IosName;
        //            break;
        //        case (int)TypefaceType.RobotoRegular:
        //            fullFontName += Fonts.CustomFont.RobotoRegular.IosName;
        //            break;
        //        case (int)TypefaceType.RobotoMedium:
        //            fullFontName += Fonts.CustomFont.RobotoMedium.IosName;
        //            break;
        //        case (int)TypefaceType.RobotoMediumItalic:
        //            fullFontName += Fonts.CustomFont.RobotoMediumItalic.IosName;
        //            break;
        //        case (int)TypefaceType.RobotoBold:
        //            fullFontName += Fonts.CustomFont.RobotoBold.IosName;
        //            break;
        //    }

        //    fullFontName += ".ttf";

        //    //Do not enclose this stream in a using block: https://stackoverflow.com/questions/48061401/drawtext-in-canvas-skiasharp-text-does-not-display
        //    Stream stream = GetManifestResourceStream(
        //        fullFontName, ResourceAssembly);

        //    var typeface = SKTypeface.FromStream(stream);
        //    typefaceCache.Add(type, typeface);
        //}
        #endregion

        public static void Initialize(Assembly resourceAssembly)
        {
            CustomResourceAssembly = resourceAssembly;
            svgCache = new Dictionary<string, SKSvg>();

            typefaceCache = new Dictionary<int, SKTypeface>();

            foreach (int i in Enum.GetValues(typeof(TypefaceType)))
            {
                // don't do anything, this isn't standardized...
                //CacheTypeface(i);
            }
        }


        /// <summary>
        /// We copied over this function from FileOperations to avoid doing a DI.Get operation before the DI is fully built
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Stream GetManifestResourceStream(string path, Assembly assembly)
        {
            Stream s = assembly.GetManifestResourceStream(path);

            if (s == null)
            {
                // try to match lower-case names


                string message = $"Looking for file {path} in assembly {assembly} but could not find it.";

                var resourceNames = assembly.GetManifestResourceNames();
                if (resourceNames.Length > 0)
                {
                    message += "\nFound names\n";
                    foreach (var name in resourceNames)
                    {
                        if (name.ToLowerInvariant() == path.ToLowerInvariant())
                        {
                            s = assembly.GetManifestResourceStream(name);
                            break;
                        }

                        message += name + "\n";
                    }
                }
                else
                {
                    message += "\nThis assembly contains no embedded resource images.";
                }


                // See if it's still null
                if (s == null)
                {
                    throw new ArgumentException(message);

                }

            }

            return s;
        }
    }
}
