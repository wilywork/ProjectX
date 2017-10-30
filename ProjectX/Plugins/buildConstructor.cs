using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProjectX.Plugins
{
    class BuildConstructor
    {
        public class Config
        {
        }

        public static Config configBuilds = new Config();

        public static string linkBundle = "https://www.deathofrust.com.br/bundles/castelo.unity3d";

        public static void Start()
        {
            //GameObject Load = new GameObject();
            //LoadObjectFromBundle test = Load.AddComponent<LoadObjectFromBundle>();
            ////test.StartCoroutine("Start");
            //UnityEngine.Object.DontDestroyOnLoad(Load);
        }

    }

    public class LoadObjectFromBundle : MonoBehaviour
    {
        bool loaded = false;

        IEnumerator Start()
        {

            Debug.Log("download...");
            var www = WWW.LoadFromCacheOrDownload(BuildConstructor.linkBundle, 1);

            //wait for download complete
            yield return www;

            if (!loaded)
            {
                loaded = true;
                //load ahd retrieve assetBundle
                AssetBundle bundle = www.assetBundle;
                try
                {
                    Debug.Log("listall..");
                    UnityEngine.Object[] loadAll = bundle.LoadAll();
                    foreach (UnityEngine.Object item in loadAll)
                    {
                        Debug.Log(item.name);
                    }

                    // load GameObject
                     GameObject Obj = bundle.Load("castelo", typeof(GameObject)) as GameObject;

                    //Instantiate
                    Instantiate(Obj, new Vector3(5556, 408, -3686), new Quaternion(50, 0, 0, 50));
                    Debug.Log("castelo!");

                }
                catch (Exception ex)
                {
                    Debug.Log("try " + ex);
                }
            }

        }
    }
}
