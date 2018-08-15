using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

[System.Serializable]
public struct Coord {
    [XmlAttribute("x")]
    public int x;
    [XmlAttribute("y")]
    public int y;

    public static readonly Coord Zero = new Coord();

    public Coord(int newX = 0, int newY = 0) {
        x = newX;
        y = newY;
    }

    public Coord(float newX, float newY) {
        x = (int)Mathf.Round(newX);
        y = (int)Mathf.Round(newY);
    }

    public static bool operator ==(Coord c1, Coord c2) {
        return c1.Equals(c2);
    }

    public static bool operator !=(Coord c1, Coord c2) {
        return !c1.Equals(c2);
    }

    public static Coord operator +(Coord c1, Coord c2) {
        return new Coord(c1.x + c2.x, c1.y + c2.y);
    }

    public static Coord operator -(Coord c1, Coord c2) {
        return new Coord(c1.x - c2.x, c1.y - c2.y);
    }

    public override bool Equals(object obj) {
        if (obj is Coord) {
            Coord val = (Coord)obj;
            return (val.x == this.x && val.y == this.y);
        }
        return base.Equals(obj);
    }

    public override int GetHashCode() {
        return x + y << 16;
    }

    public override string ToString() {
        return string.Format("({0}: {1})", x, y);
    }
}
