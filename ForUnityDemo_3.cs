using UnityEngine;

using System.Collections;



public class GoldPath : MonoBehaviour {
    GameObject musiceffectManager;
    public GameObject hitgoldScore;
    Camera worldCamera;
    Camera guiCamera;
    TweenScale ivGold;
    Vector3[] paths;
	// Use this for initialization

    void Start()
    {
        musiceffectManager = GameObject.Find("MusicEffectManager");
        ivGold = GameObject.Find("iv_Gold").GetComponent<TweenScale>();
        worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        guiCamera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    public void SetGold(Transform t) {
        Vector3 pos = worldCamera.WorldToViewportPoint(t.transform.position);
        if (pos.z >= 0)
        {
            pos = guiCamera.ViewportToWorldPoint(pos);
            pos.z = 0;
            transform.position = pos;
        }
        else
        {
            pos = guiCamera.ViewportToWorldPoint(pos);
            pos.z = guiCamera.farClipPlane + 10f;
            transform.position = pos;
        }

        paths = new Vector3[3];
        paths[0] = new Vector3(pos.x - Random.Range(-0.5f, 0.5f), pos.y - Random.Range(-0.2f, 0.1f), 0);
        paths[1] = new Vector3(pos.x - Random.Range(-0.3f, 0.3f), pos.y - Random.Range(-0.5f, -0.1f), 0);
        paths[2] = new Vector3(-1.3f, 0.9f, 0);
        iTween.MoveTo(gameObject, iTween.Hash("path", paths, "time", 0.8f, "easeType", iTween.EaseType.easeInCubic)); 

        GameObject Gold_score = NGUITools.AddChild(GameObject.Find("Camera"), hitgoldScore);
        Gold_score.GetComponent<HitScore>().SetGold(paths[0]);
        Gold_score.GetComponent<UILabel>().text = "+" + Random.Range(1, 32);

        StartCoroutine(GoldAnimation());
    
    }

		


    IEnumerator GoldAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        ivGold.ResetToBeginning();
        ivGold.PlayForward();
        musiceffectManager.SendMessage("PlayCoin");
        Destroy(gameObject);
    }

}
