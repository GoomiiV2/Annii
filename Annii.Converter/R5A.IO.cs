using System.Numerics;
using System.Text;

namespace Annii.Converter;

public partial class R5A
{

    public static R5A LoadFromFile(string filePath)
    {
        var r5a              = new R5A();
        var currentReadState = ReadState.Version;
        Track currentTrack     = null;
        Object currentObject = null;
        Key currentKey = null;
        
        try
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines) {
                var r5aLine = line.Trim();
                if (r5aLine == "-Info-")
                    currentReadState = ReadState.Info;
                else if (r5aLine == "-GlobalCounts-")
                    currentReadState = ReadState.GlobalCounts;
                else if (r5aLine == "-Camera-")
                    currentReadState = ReadState.Camera;
                else if (r5aLine == "-Target-")
                    currentReadState = ReadState.Target;
                else if (r5aLine == "-Track-") {
                    currentReadState = ReadState.Tracks;
                    if (currentTrack != null)
                        r5a.Tracks.Add(currentTrack);
                    currentTrack     = new Track();
                }
                else if (r5aLine == "-Object-") {
                    currentReadState = ReadState.Objects;
                    if (currentObject != null)
                        r5a.Objects.Add(currentObject);
                    currentObject = new Object();

                    if (currentTrack != null) {
                        r5a.Tracks.Add(currentTrack);
                        currentTrack = null;
                    }
                }
                else if (r5aLine == "-Verts-") {
                    currentReadState = ReadState.Vert;
                    continue;
                }
                else if (r5aLine == "-Faces-") {
                    currentReadState = ReadState.Face;
                    continue;
                }
                else if (r5aLine == "-Key-") {
                    currentReadState = ReadState.Key;
                    if (currentKey != null)
                        currentObject.Keys.Add(currentKey);
                    currentKey = new Key();
                }
                else if (r5aLine == "-EndKeys-")
                    currentObject.Keys.Add(currentKey);
                else if (r5aLine == "-End-")
                    r5a.Objects.Add(currentObject);
                else if (r5aLine == "-BeginKeys-")
                    continue;

                var split = r5aLine.Split(':');
                var key = split[0].Trim();
                var value = split.Length > 1 ? split[1].Trim() : "";
                
                switch (currentReadState) {
                    case ReadState.Version:
                        r5a.Version = r5aLine;
                        break;
                    case ReadState.Info:
                        if (key == "Name")
                            r5a.Info.Name = value;
                        else if (key == "Author")
                            r5a.Info.Author = value;
                        else if (key == "Creation-Date")
                            r5a.Info.CreationDate = value;
                        break;
                    case ReadState.GlobalCounts:
                        if (key == "NumObjects")
                            r5a.GlobalCounts.NumObjects = int.Parse(value);
                        else if (key == "NumFrames")
                            r5a.GlobalCounts.NumFrames = int.Parse(value);
                        else if (key == "NumTracks")
                            r5a.GlobalCounts.NumTracks = int.Parse(value);
                        else if (key == "DefaultFPS")
                            r5a.GlobalCounts.DefaultFPS = float.Parse(value);
                        break;
                    case ReadState.Camera:
                        if (key == "Position")
                            r5a.Camera.Position = ParseVector3(value);
                        else if (key == "WorldUp")
                            r5a.Camera.WorldUp = ParseVector3(value);
                        else if (key == "LeftHanded")
                            r5a.Camera.LeftHanded = value == "true";
                        else if (key == "Orthographic")
                            r5a.Camera.Orthographic = value == "true";
                        break;
                    case ReadState.Target:
                        if (key == "Position")
                            r5a.Target.Position = ParseVector3(value);
                        else if (key == "Width")
                            r5a.Target.Width = float.Parse(value);
                        else if (key == "Height")
                            r5a.Target.Height = float.Parse(value);
                        break;
                    case ReadState.Tracks:
                        if (key == "Name")
                            currentTrack.Name = value;
                        else if (key == "StartFrame")
                            currentTrack.StartFrame = int.Parse(value);
                        else if (key == "FrameCount")
                            currentTrack.FrameCount = int.Parse(value);
                        break;
                    case ReadState.Objects:
                        if (key == "Name")
                            currentObject.Name = value;
                        else if (key == "NumVerts")
                            currentObject.NumVerts = int.Parse(value);
                        else if (key == "NumFaces")
                            currentObject.NumFaces = int.Parse(value);
                        else if (key == "NumKeys")
                            currentObject.NumKeys = int.Parse(value);
                        break;
                    case ReadState.Vert:
                        currentObject.Verts.Add(ParseVert(r5aLine));
                        break;
                    case ReadState.Face:
                        var faces = line.Split(' ');
                        var face = new Face();
                        face.V1 = int.Parse(faces[0]);
                        face.V2 = int.Parse(faces[1]);
                        face.V3 = int.Parse(faces[2]);
                        currentObject.Faces.Add(face);
                        break;
                    case ReadState.Key:
                        if (key == "Frame")
                            currentKey.Frame = int.Parse(value);
                        else if (key == "Position")
                            currentKey.Position = ParseKeyProp<Vector3>(value);
                        else if (key == "Rotation")
                            currentKey.Rotation = ParseKeyProp<Vector3>(value);
                        else if (key == "Scale")
                            currentKey.Scale = ParseKeyProp<Vector3>(value);
                        else if (key == "UOffset")
                            currentKey.UOffset = ParseKeyProp<float>(value);
                        else if (key == "VOffset")
                            currentKey.VOffset = ParseKeyProp<float>(value);
                        else if (key == "Opacity")
                            currentKey.Opacity = ParseKeyProp<float>(value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return r5a;
    }
    
    public bool ExportToObj(string filePath)
    {
        try
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("# OBJ file exported from R5A");
            writer.WriteLine($"# Original file: {Info.Name}");
            writer.WriteLine($"# Author: {Info.Author}");
            writer.WriteLine($"# Creation date: {Info.CreationDate}");
            writer.WriteLine($"# Export date: {DateTime.Now}");
            writer.WriteLine();
            
            int vertexOffset = 1; 
            foreach (var obj in Objects)
            {
                writer.WriteLine($"o {obj.Name}");
                foreach (var vert in obj.Verts)
                    writer.WriteLine($"v {vert.Position.X} {vert.Position.Y} {vert.Position.Z}");
                
                foreach (var vert in obj.Verts)
                    writer.WriteLine($"vt {vert.Uv.X} {1 - vert.Uv.Y}");
                
                foreach (var face in obj.Faces)
                {
                        writer.WriteLine($"f {face.V1 + vertexOffset}/{face.V1 + vertexOffset} " +
                                         $"{face.V2 + vertexOffset}/{face.V2 + vertexOffset} " +
                                         $"{face.V3 + vertexOffset}/{face.V3 + vertexOffset}");
                }
                
                vertexOffset += obj.Verts.Count;
                writer.WriteLine();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting to OBJ: {ex.Message}");
            return false;
        }
    }

    private static Vector3 ParseVector3(string value)
    {
        var vec = new Vector3();
        var floats = value.Trim('(', ')').Split(' ');
        
        vec.X = float.Parse(floats[0].Trim());
        vec.Y = float.Parse(floats[1].Trim());
        vec.Z = float.Parse(floats[2].Trim());
        
        return vec;
    }
    
    private static Vector2 ParseVector2(string value)
    {
        var vec    = new Vector2();
        var floats = value.Trim('(', ')').Split(' ');
        
        vec.X = float.Parse(floats[0].Trim());
        vec.Y = float.Parse(floats[1].Trim());
        
        return vec;
    }

    public static Vert ParseVert(string value)
    {
        var split = value.Split(") (");
        
        var vert  = new Vert();
        vert.Position = ParseVector3(split[0]);
        vert.Uv = ParseVector2(split[1]);
        
        return vert;
    }

    private static KeyProp<T> ParseKeyProp<T>(string value)
    {
        var split = value.Split(") ");
        KeyProp<T> keyProp = new KeyProp<T>();

        if (typeof(T) == typeof(Vector3))
            keyProp.Value = (T)(object)ParseVector3(split[0]);
        else if (typeof(T) == typeof(Vector2))
            keyProp.Value = (T)(object)ParseVector2(split[0]);
        else if (typeof(T) == typeof(float))
            keyProp.Value = (T)(object)float.Parse(split[0].Trim('('));
        else if (typeof(T) == typeof(int))
            keyProp.Value = (T)(object)int.Parse(split[0].Trim('('));
        
        keyProp.Mode = (InterpMode)Enum.Parse(typeof(InterpMode), split[1]);

        return keyProp;
    }

    public enum ReadState
    {
        Version,
        Info,
        GlobalCounts,
        Camera,
        Target,
        Tracks,
        Objects,
        Vert,
        Face,
        Key,
    }
}