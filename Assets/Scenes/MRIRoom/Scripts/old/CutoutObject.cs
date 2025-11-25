using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField]
    private Transform targetObject;

    [SerializeField]
    private LayerMask wallMask;

    [SerializeField]
    private Transform cutoutSource;

    private void Update()
    {
        Vector2 cutoutPos = cutoutSource.position;
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = targetObject.position - cutoutSource.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(cutoutSource.position, offset, offset.magnitude, wallMask);

        for (int i = 0; i < hitObjects.Length; ++i)
        {
            Material[] materials = hitObjects[i].transform.GetComponent<Renderer>().materials;

            for (int m = 0; m < materials.Length; ++m)
            {
                materials[m].SetVector("_CutoutPos", cutoutPos);
                materials[m].SetFloat("_CutoutSize", 0.2f);
                materials[m].SetFloat("_FalloffSize", 0.05f);
            }
        }
    }
}
