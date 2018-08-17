// (c) Copyright HutongGames, LLC 2010-2014. All rights reserved.
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Array)]
    [Tooltip("Add an item to the end of an Array.")]
    public class LoadPicFromDisc : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("The Array Variable to use.")]
        public FsmString picPath;

        public FsmString textureExtension = "png";


        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("The Array Variable to use.")]
        public FsmArray textureArr;


        public FsmBool picLoaded = false;

        public override void Reset()
        {
            textureArr = null;
        }

        public override void OnEnter()
        {
            StartCoroutine(LoadTexture());
        }

        IEnumerator LoadTexture()
        {
            picLoaded.Value = false;
            // 大鱼
            string[] filePaths = Directory.GetFiles(
                Application.streamingAssetsPath + @"/" + picPath.Value,
                "*." + textureExtension.Value);

            Dictionary<int, string> dicTemp = new Dictionary<int, string>();

            int[] indexArray = new int[filePaths.Length];
            for ( int idx=0; idx < filePaths.Length; idx++ )
            {
                string[] aSs = filePaths[idx].Split('.');
                string[] aSs1 = aSs[aSs.Length - 2].Split('/');
                indexArray[idx] = int.Parse(aSs1[aSs1.Length - 1]);
                //Debug.Log("indexArray[idx]: " + indexArray[idx] );

                dicTemp.Add(indexArray[idx], filePaths[idx]);
            }
            // 排序
            System.Array.Sort(indexArray);

            textureArr.Resize(filePaths.Length);

            Debug.Log("PhotoLoader found " + filePaths.Length + " images in " + picPath.Value);

            int i = 0;
            for (i = 0; i < indexArray.Length; i++)
            {
                WWW www = new WWW("file://" + dicTemp[indexArray[i]]);

                yield return www;
                if (www.isDone)
                {
                    textureArr.Set(i, www.texture);

                    Debug.Log("filePaths: " + dicTemp[indexArray[i]]);
                }
            }
            if (i >= indexArray.Length)
            {
                picLoaded.Value = true;
                Debug.Log(picPath.Value + "loaded");
            }
        }

    }

}

