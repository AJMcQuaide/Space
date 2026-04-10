using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class GameDataReader
{
    BinaryReader reader;

    public GameDataReader(BinaryReader reader)
    {
        this.reader = reader;
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public int ReadInt32()
    {
        return reader.ReadInt32();
    }

    public Quaternion ReadQuaternion()
    {
        Quaternion quaterion;
        quaterion.x = reader.ReadSingle();
        quaterion.y = reader.ReadSingle();
        quaterion.z = reader.ReadSingle();
        quaterion.w = reader.ReadSingle();
        return quaterion;
    }

    public Vector3 ReadVector3()
    {
        Vector3 vector;
        vector.x = reader.ReadSingle();
        vector.y = reader.ReadSingle();
        vector.z = reader.ReadSingle();
        return vector;
    }

    public double3 ReadDouble3()
    {
        double3 d3;
        d3.x = reader.ReadDouble();
        d3.y = reader.ReadDouble();
        d3.z = reader.ReadDouble();
        return d3;
    }

    public double ReadDouble()
    {
        return reader.ReadDouble();
    }

    public Color ReadColor()
    {
        Color color;
        color.r = reader.ReadSingle();
        color.g = reader.ReadSingle();
        color.b = reader.ReadSingle();
        color.a = reader.ReadSingle();
        return color;
    }

    public bool ReadBool()
    {
        return reader.ReadBoolean();
    }

    public string ReadString()
    {
        return reader.ReadString();
    }
}
