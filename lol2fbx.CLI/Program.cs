using BCnEncoder.Shared;
using CommandLine;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Environment;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Meta;
using LeagueToolkit.Toolkit;
using Assimp;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using LeagueTexture = LeagueToolkit.Core.Renderer.Texture;

namespace lol2fbx.CLI;

class Program
{
    static void Main(string[] args)
    {
        CommandLine.Parser.Default
            .ParseArguments<SkinnedMeshToFbxOptions, MapGeometryToFbxOptions, FbxToSkinnedMeshOptions>(args)
            .MapResult(
                (SkinnedMeshToFbxOptions opts) => ConvertSkinnedMeshToFbx(opts),
                (MapGeometryToFbxOptions opts) => ConvertMapGeometryToFbx(opts),
                (FbxToSkinnedMeshOptions opts) => ConvertFbxToSkinnedMesh(opts),
                HandleErrors
            );
    }

    private static int HandleErrors(IEnumerable<Error> errors)
    {
        return -1;
    }

    private static int ConvertSkinnedMeshToFbx(SkinnedMeshToFbxOptions options)
    {
        if (options.MaterialNames.Count() != options.TexturePaths.Count())
            throw new InvalidOperationException("Material name count and Animation path count must be equal");

        IEnumerable<(string, Stream)> textures = options.MaterialNames
            .Zip(options.TexturePaths)
            .Select(x =>
            {
                using FileStream textureFileStream = File.OpenRead(x.Second);
                LeagueTexture texture = LeagueTexture.Load(textureFileStream);

                ReadOnlyMemory2D<ColorRgba32> mipMap = texture.Mips[0];
                using ImageSharpImage image = mipMap.ToImage();

                MemoryStream imageStream = new();
                image.SaveAsPng(imageStream);
                imageStream.Seek(0, SeekOrigin.Begin);

                return (x.First, (Stream)imageStream);
            });

        IEnumerable<(string, IAnimationAsset)> animations = !string.IsNullOrEmpty(options.AnimationsPath)
            ? LoadAnimations(options.AnimationsPath)
            : Enumerable.Empty<(string, IAnimationAsset)>();

        using FileStream skeletonStream = File.OpenRead(options.SkeletonPath);

        SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(options.SimpleSkinPath);
        RigResource skeleton = new(skeletonStream);

        // Convert to FBX using Assimp
        Scene scene = simpleSkin.ToFbxScene(skeleton, textures, animations);
        AssimpContext exporter = new();
        exporter.ExportFile(scene, options.FbxPath, "fbx");

        return 1;
    }

    private static int ConvertFbxToSkinnedMesh(FbxToSkinnedMeshOptions options)
    {
        string skeletonPath = string.IsNullOrEmpty(options.SkeletonPath) switch
        {
            true => Path.ChangeExtension(options.SimpleSkinPath, "skl"),
            false => options.SkeletonPath
        };

        AssimpContext importer = new();
        Scene scene = importer.ImportFile(options.FbxPath, PostProcessSteps.None);

        var (simpleSkin, rig) = scene.ToRiggedMesh();

        using FileStream simpleSkinStream = File.Create(options.SimpleSkinPath);
        simpleSkin.WriteSimpleSkin(simpleSkinStream);

        using FileStream rigStream = File.Create(skeletonPath);
        rig.Write(rigStream);

        return 1;
    }

    private static int ConvertMapGeometryToFbx(MapGeometryToFbxOptions options)
    {
        MapGeometryFbxConversionContext conversionContext =
            new(
                MetaEnvironment.Create(
                    Assembly.Load("LeagueToolkit.Meta.Classes").GetExportedTypes().Where(x => x.IsClass)
                ),
                new()
                {
                    FlipAcrossX = options.FlipAcrossX,
                    GameDataPath = options.GameDataPath,
                    LayerGroupingPolicy = options.LayerGroupingPolicy,
                    TextureQuality = options.TextureQuality
                }
            );

        using FileStream environmentAssetStream = File.OpenRead(options.MapGeometryPath);
        using FileStream materialsBinStream = File.OpenRead(options.MaterialsBinPath);

        using EnvironmentAsset mapGeometry = new(environmentAssetStream);
        BinTree materialsBin = new(materialsBinStream);

        // Convert to FBX using Assimp
        Scene scene = mapGeometry.ToFbxScene(materialsBin, conversionContext);
        AssimpContext exporter = new();
        exporter.ExportFile(scene, options.FbxPath, "fbx");

        return 1;
    }

    private static IEnumerable<(string, IAnimationAsset)> LoadAnimations(string path) =>
        Directory
            .EnumerateFiles(path, "*.anm")
            .Select(animationPath =>
            {
                using FileStream stream = File.OpenRead(animationPath);
                return (Path.GetFileNameWithoutExtension(animationPath), AnimationAsset.Load(stream));
            });
}
