using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GameVersion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		GetComponent<Text>().text = string.Format("v{0}", NetworkLobby.GameVersion);
	}
}
