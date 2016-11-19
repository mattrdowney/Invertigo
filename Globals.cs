using UnityEngine;
using System.Collections;

public static class Globals
{
	public static float radius = 0.025f; // * (64^2) / (x^2) for rooms other than room64

    //8 subdivisions latitude (final BOSS)
    //+16 subdivisions (final corridor)
    //24 subdivisions (final stage)
    //+8 subdivisions corridor
    //32
    //40
    //48
    //56
    //64 subdivisions (first stage)
}
