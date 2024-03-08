using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tool
{
    public static void Swap<T>(this T[] array, int index1, int index2)
    {
        T temp = array[index2];
        array[index2] = array[index1];
        array[index1] = temp;
    }

    public static void Swap<T>(this List<T> list, int index1, int index2)
    {
        T temp = list[index2];
        list[index2] = list[index1];
        list[index1] = temp;
    }

    public static void SortRandom<T>(this T[] array)
    {
        int randomIndex;
        for (int i = array.Length - 1; i > 0; i--)
        {
            randomIndex = Random.Range(0, i);
            array.Swap(randomIndex, i);
        }
    }

    public static void SortRandom<T>(this List<T> list)
    {
        int randomIndex;
        for (int i = list.Count - 1; i > 0; i--)
        {
            randomIndex = Random.Range(0, i);
            list.Swap(randomIndex, i);
        }
    }

    //-------------------------------------------------
    // Remap num from range 1 to range 2
    //-------------------------------------------------
    public static float RemapNumber(float num, float low1, float high1, float low2, float high2)
    {
        return low2 + (num - low1) * (high2 - low2) / (high1 - low1);
    }


    //-------------------------------------------------
    public static float RemapNumberClamped(float num, float low1, float high1, float low2, float high2)
    {
        return Mathf.Clamp(RemapNumber(num, low1, high1, low2, high2), Mathf.Min(low2, high2), Mathf.Max(low2, high2));
    }

    //-------------------------------------------------
    public static bool IsNullOrEmpty<T>(T[] array)
    {
        if (array == null)
            return true;

        if (array.Length == 0)
            return true;

        return false;
    }

    //-------------------------------------------------
    public static void ResetTransform(Transform t, bool resetScale = true)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        if (resetScale)
        {
            t.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
