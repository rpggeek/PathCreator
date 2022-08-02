using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [Range(1, 7)]
    public int smoothiness;

    private List<Vector3> positions = new List<Vector3>();

    public void Generate(List<Vector3> positions)
    {
        GameObject pivot = new GameObject();
        pivot.name = "paths";
        pivot.transform.SetParent(gameObject.transform);

        foreach(Vector3 pos in positions)
        {
            GameObject obj = new GameObject();
            obj.transform.position = pos;

            obj.name = "path" + positions.IndexOf(pos);
            obj.transform.SetParent(pivot.transform);
        }
    }

    public void ClearAll()
    {
        foreach(Transform tf in gameObject.transform)
        {
            DestroyImmediate(tf.gameObject);
        }
        positions.Clear();
    }
}
