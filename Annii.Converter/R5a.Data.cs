using System.Numerics;

namespace Annii.Converter;

public partial class R5A
{
    public string       Version;
    public R5AInfo      Info;
    public GlobalCounts GlobalCounts;
    public Camera       Camera;
    public Target       Target;
    public List<Track>  Tracks = [];
    public List<Object> Objects = [];
}

public struct R5AInfo
{
    public string Name;
    public string Author;
    public string CreationDate;
}

public struct GlobalCounts
{
    public int NumObjects;
    public int NumFrames;
    public int NumTracks;
    public float DefaultFPS;
}

public struct Camera
{
    public Vector3 Position;
    public Vector3 WorldUp;
    public bool    LeftHanded;
    public bool    Orthographic;
}

public struct Target
{
    public Vector3 Position;
    public float   Width;
    public float   Height;
}

public class Track
{
    public string Name;
    public int    StartFrame;
    public int    FrameCount;
}

public class Object
{
    public string     Name;
    public int        NumVerts;
    public int        NumFaces;
    public int        NumKeys;
    public List<Vert> Verts = [];
    public List<Face> Faces = [];
    public List<Key>  Keys = [];
}

public struct Vert
{
    public Vector3 Position;
    public Vector2 Uv;
}

public struct Face
{
    public int V1;
    public int V2;
    public int V3;
}

public class Key
{
    public int              Frame;
    public KeyProp<Vector3> Position;
    public KeyProp<Vector3> Rotation;
    public KeyProp<Vector3> Scale;
    public KeyProp<float>   UOffset;
    public KeyProp<float>   VOffset;
    public KeyProp<float>   Opacity;
}

public class KeyProp<T>
{
    public T Value;
    public InterpMode Mode;
}

public enum InterpMode
{
    Linear,
}