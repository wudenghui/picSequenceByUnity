
using UnityEngine;
using UnityEngine;
using System.Collections;
using System.IO;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Array)]
    [Tooltip("Sets a named texture in a game object's material.")]
    public class MyPlayPicSequence : ComponentAction<Renderer>
    {
        [Tooltip("The GameObject that the material is applied to.")]
        [CheckForComponent(typeof(Renderer))]
        public FsmOwnerDefault gameObject;

        [Tooltip("GameObjects can have multiple materials. Specify an index to target a specific material.")]
        public FsmInt materialIndex;

        [Tooltip("Alternatively specify a Material instead of a GameObject and Index.")]
        public FsmMaterial material;

        [UIHint(UIHint.NamedTexture)]
        [Tooltip("A named parameter in the shader.")]
        public FsmString namedTexture;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("序列帧数组")]
        public FsmArray textureArr;

        [Tooltip("多少帧/秒")]
        public FsmInt framesPerSec = 30;

        float curMovieTime = 0;   // 默认是0秒    主体程序的控制变量

        public FsmBool isResetOnEnter = true;   // 默认是5秒
        public FsmBool isLoop = true;   // 默认是5秒
        // 影片的各相关参数
        float maxMovieTime = 5;   // 默认是5秒
        private float startTime = 0, endTime = 5;
        private int curIndex = 0, preIndex = -1;
        private bool over = false;
        public override void Reset()
        {
            gameObject = null;
            materialIndex = 0;
            material = null;
            namedTexture = "_MainTex";
            textureArr = null;
            over = false;
        }

        public override void OnEnter()
        {
            maxMovieTime = textureArr.Length * 1.0f/ framesPerSec.Value;

            startTime = Time.time; // 这里的时间是float类型的，有溢出的可能，如果要长时间运行程序，需要做处理
            maxMovieTime = maxMovieTime > 0.1f ? maxMovieTime : 0.1f;
            endTime = startTime + maxMovieTime;

            if (isResetOnEnter.Value)
            {
                curMovieTime = startTime;
            }
            over = false;

            DoPlayMovie();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            DoPlayMovie();
        }

        void DoPlayMovie()
        {
            if (textureArr.Length <= 0)
                return;
            if ( over )
                return;

            curMovieTime = Time.time; // 这里的时间是float类型的，有溢出的可能，如果要长时间运行程序，需要做处理

            if (isLoop.Value)
            {
                // 循环
                if (curMovieTime >= endTime)
                {
                    startTime = Time.time; // 这里的时间是float类型的，有溢出的可能，如果要长时间运行程序，需要做处理
                    maxMovieTime = maxMovieTime > 0.1f ? maxMovieTime : 0.1f;
                    endTime = startTime + maxMovieTime;
                }
            }
            else
            {
                // 不循环
                if (curMovieTime >= endTime)
                {
                    over = true;
                }
            }

            curIndex = (int)(
                Mathf.Clamp( (curMovieTime - startTime) / (endTime - startTime), 0, 1 )
                * (textureArr.Length - 1 )
                );
            
            if (preIndex != curIndex)
            {
                preIndex = curIndex;
                //Debug.Log("curIndex:" + curIndex);

                var namedTex = namedTexture.Value;
                if (namedTex == "") namedTex = "_MainTex";

                if (material.Value != null)
                {
                    material.Value.SetTexture(namedTex, (Texture2D)textureArr.Get(curIndex));
                    return;
                }

                var go = Fsm.GetOwnerDefaultTarget(gameObject);
                if (!UpdateCache(go))
                {
                    return;
                }

                if (renderer.material == null)
                {
                    LogError("Missing Material!");
                    return;
                }

                if (materialIndex.Value == 0)
                {
                    renderer.material.SetTexture(namedTex, (Texture2D)textureArr.Get(curIndex));
                }
                else if (renderer.materials.Length > materialIndex.Value)
                {
                    var materials = renderer.materials;
                    materials[materialIndex.Value].SetTexture(namedTex, (Texture2D)textureArr.Get(curIndex));
                    renderer.materials = materials;
                }
            }
        }
    }
}