﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DropVeg : MonoBehaviour
{

    private bool dropped;
    private List<string> prefabs = new List<string>();
    private string prefabsPath;

    private Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        dropped = false;
        transform.Find("VegUp").GetComponent<VegSelection>().setSize(prefabs.Count);
        transform.Find("VegDown").GetComponent<VegSelection>().setSize(prefabs.Count);

    }

    // Update is called once per frame
    void Update()
    {

        RaycastHit hitInfo = new RaycastHit();

        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {

            Collider collider = hitInfo.collider;

            if (Input.GetMouseButtonDown(0) && (collider.gameObject).Equals(GameObject.Find("Vegetation").gameObject))
            {

                if (dropped)
                {
                    close();
                }
                else
                {
                    open();
                }

            }

        }

    }

    public void loadPrefabs()
    {

        try
        {

            DirectoryInfo Dir = new DirectoryInfo(prefabsPath);
            FileInfo[] FileList = Dir.GetFiles("*.prefab", SearchOption.AllDirectories);

            foreach (FileInfo FI in FileList)
            {
                prefabs.Add(FI.FullName.Substring(FI.FullName.IndexOf("\\LowPolyNaturePackLite")));
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }

    }

    public bool getDropped()
    {

        return dropped;

    }

    public void setDropped(bool state)
    {

        dropped = state;

    }

    public void open()
    {

        GameObject vegUp = GameObject.Find("VegUp").gameObject;
        vegUp.transform.localPosition = new Vector3(0f, 1.1f, 0f);

        GameObject vegDown = GameObject.Find("VegDown").gameObject;
        vegDown.transform.localPosition = new Vector3(0f, -1.1f, 0f);

        dropped = true;

    }

    public void close()
    {

        GameObject vegUp = GameObject.Find("VegUp").gameObject;
        vegUp.transform.localPosition = new Vector3(0f, 0f, 5f);

        GameObject vegDown = GameObject.Find("VegDown").gameObject;
        vegDown.transform.localPosition = new Vector3(0f, 0f, 10f);

        dropped = false;

    }

}
