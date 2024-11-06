using System;
using System.Collections.Generic;
using CommandLine;
using LeagueToolkit.IO.MapGeometryFile;

namespace lol2fbx.CLI
{
    [Verb("skn2fbx", HelpText = "Converts a Skinned Mesh (skn, skl, anm) into an FBX asset")]
    public class SkinnedMeshToFbxOptions
    {
        [Option('m', "skn", Required = true, HelpText = "Simple Skin (.skn) path")]
        public string SimpleSkinPath { get; set; }

        [Option('s', "skl", Required = true, HelpText = "Skeleton (.skl) path")]
        public string SkeletonPath { get; set; }

        [Option(
            'f',
            "fbx",
            Required = true,
            HelpText = "Path of the generated FBX file"
        )]
        public string FbxPath { get; set; }

        [Option('a', "anm", Required = false, HelpText = "Animations (.anm) folder path")]
        public string AnimationsPath { get; set; }

        [Option("materials", Required = false, HelpText = "Simple Skin material names for textures")]
        public IEnumerable<string> MaterialNames { get; set; }

        [Option("textures", Required = false, HelpText = "Texture paths for the specified materials")]
        public IEnumerable<string> TexturePaths { get; set; }
    }

    [Verb("fbx2skn", HelpText = "Converts an FBX asset into a Skinned Mesh (.skn, .skl)")]
    public class FbxToSkinnedMeshOptions
    {
        [Option('f', "fbx", Required = true, HelpText = "FBX Asset (.fbx) path")]
        public string FbxPath { get; set; }

        [Option('m', "skn", Required = true, HelpText = "Simple Skin (.skn) path")]
        public string SimpleSkinPath { get; set; }

        [Option(
            's',
            "skl",
            Required = false,
            HelpText = "Skeleton (.skl) path (if not specified, will be saved under the same name as the Simple Skin)"
        )]
        public string SkeletonPath { get; set; }
    }

    [Verb("mapgeo2fbx", HelpText = "Converts Map Geometry into an FBX asset")]
    public class MapGeometryToFbxOptions
    {
        [Option('m', "mgeo", Required = true, HelpText = "Map Geometry (.mapgeo) path")]
        public string MapGeometryPath { get; set; }

        [Option('b', "matbin", Required = true, HelpText = "Materials Bin (.materials.bin) path")]
        public string MaterialsBinPath { get; set; }

        [Option(
            'f',
            "fbx",
            Required = true,
            HelpText = "Path of the generated FBX file"
        )]
        public string FbxPath { get; set; }

        [Option('d', "gamedata", Required = false, HelpText = "Game Data path (required for bundling textures)")]
        public string GameDataPath { get; set; }

        [Option('x', "flipX", Required = false, Default = true, HelpText = "Whether to flip the map node's X axis")]
        public bool FlipAcrossX { get; set; }

        [Option(
            'l',
            "layerGroupingPolicy",
            Required = false,
            Default = MapGeometryFbxLayerGroupingPolicy.Default,
            HelpText = $"The layer grouping policy for meshes (use `Ignore` if you don't want to group meshes based on layers)"
        )]
        public MapGeometryFbxLayerGroupingPolicy LayerGroupingPolicy { get; set; }

        [Option(
            'q',
            "textureQuality",
            Required = false,
            Default = MapGeometryFbxTextureQuality.Low,
            HelpText = $"The quality of textures to bundle (Low = 4x, Medium = 2x)"
        )]
        public MapGeometryFbxTextureQuality TextureQuality { get; set; }
    }
}
