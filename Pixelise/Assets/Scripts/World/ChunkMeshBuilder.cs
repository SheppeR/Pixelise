using System.Collections.Generic;
using Pixelise.Core.Blocks;
using Pixelise.Core.Math;
using Pixelise.Core.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace World
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkMeshBuilder : MonoBehaviour
    {
        private static readonly Vector3Int[] FaceChecks =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.right,
            Vector3Int.left
        };

        private static readonly Vector3[,] FaceVertices =
        {
            { new(0, 1, 0), new(0, 1, 1), new(1, 1, 1), new(1, 1, 0) },
            { new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(0, 0, 1) },
            { new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1) },
            { new(1, 0, 0), new(0, 0, 0), new(0, 1, 0), new(1, 1, 0) },
            { new(1, 0, 1), new(1, 0, 0), new(1, 1, 0), new(1, 1, 1) },
            { new(0, 0, 0), new(0, 0, 1), new(0, 1, 1), new(0, 1, 0) }
        };

        [SerializeField]
        private Material blockMaterial;

        [SerializeField]
        private Material waterMaterial;

        private MeshCollider meshCollider;

        private MeshFilter solidFilter;
        private Mesh solidMesh;
        private MeshRenderer solidRenderer;

        private MeshFilter waterFilter;
        private Mesh waterMesh;
        private MeshRenderer waterRenderer;

        private void Awake()
        {
            solidFilter = GetComponent<MeshFilter>();
            solidRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();

            solidMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            solidFilter.mesh = solidMesh;
            solidRenderer.material = blockMaterial;

            var waterObj = new GameObject("WaterMesh");
            waterObj.transform.SetParent(transform, false);

            waterFilter = waterObj.AddComponent<MeshFilter>();
            waterRenderer = waterObj.AddComponent<MeshRenderer>();
            waterRenderer.material = waterMaterial;

            waterMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            waterFilter.mesh = waterMesh;
        }

        public void Build(ChunkData data)
        {
            var solidVertices = new List<Vector3>();
            var solidTriangles = new List<int>();
            var solidUVs = new List<Vector2>();

            var waterVertices = new List<Vector3>();
            var waterTriangles = new List<int>();

            var solidIndex = 0;
            var waterIndex = 0;

            for (var x = 0; x < ChunkData.Width; x++)
            for (var y = 0; y < ChunkData.Height; y++)
            for (var z = 0; z < ChunkData.Depth; z++)
            {
                var block = data.Get(new Int3(x, y, z));
                if (block == BlockType.Air)
                {
                    continue;
                }

                var pos = new Vector3(x, y, z);

                // 🌊 Eau (surface uniquement)
                if (block == BlockType.Water)
                {
                    if (y + 1 < ChunkData.Height &&
                        data.Get(new Int3(x, y + 1, z)) == BlockType.Water)
                    {
                        continue;
                    }

                    AddFace(
                        waterVertices,
                        waterTriangles,
                        ref waterIndex,
                        pos,
                        0
                    );
                    continue;
                }

                // 🧱 Blocs solides
                for (var face = 0; face < 6; face++)
                {
                    var n = new Int3(
                        x + FaceChecks[face].x,
                        y + FaceChecks[face].y,
                        z + FaceChecks[face].z
                    );

                    var neighbor = data.Get(n);
                    var current = block;


                    // AIR : jamais de mesh
                    if (current == BlockType.Air)
                    {
                        continue;
                    }

                    // EAU : visible UNIQUEMENT vers l'air
                    if (current == BlockType.Water)
                    {
                        if (neighbor != BlockType.Air)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // SOLIDE : visible si le voisin n'est pas solide
                        if (BlockRegistry.Get(neighbor).IsSolid)
                        {
                            continue;
                        }
                    }

                    AddFace(
                        solidVertices,
                        solidTriangles,
                        ref solidIndex,
                        pos,
                        face
                    );

                    solidUVs.AddRange(BlockUV.GetUVs(block, (BlockFace)face));
                }
            }

            solidMesh.Clear();
            solidMesh.SetVertices(solidVertices);
            solidMesh.SetTriangles(solidTriangles, 0);
            solidMesh.SetUVs(0, solidUVs);
            solidMesh.RecalculateNormals();

            waterMesh.Clear();
            waterMesh.SetVertices(waterVertices);
            waterMesh.SetTriangles(waterTriangles, 0);
            waterMesh.RecalculateNormals();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = solidMesh;
        }

        private static void AddFace(
            List<Vector3> verts,
            List<int> tris,
            ref int index,
            Vector3 pos,
            int face
        )
        {
            for (var i = 0; i < 4; i++)
            {
                verts.Add(pos + FaceVertices[face, i]);
            }

            tris.Add(index);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index);
            tris.Add(index + 2);
            tris.Add(index + 3);

            index += 4;
        }
    }
}