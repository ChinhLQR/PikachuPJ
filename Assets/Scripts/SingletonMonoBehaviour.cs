using UnityEngine;
using System.Collections;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	// Singleton instance
	static public SingletonMonoBehaviour<T> Instance { get; private set; }

	// This method is called when the script is loaded
	protected void Awake () {
		// Check for other instance
		if (Instance == null) {
			// No? Let me be one
			Instance = this;
		} else {
			// Already has one? I better destroy myself
			Destroy (this);
		}
	}

	// This method is called when the script is destroy
	protected void OnDestroy () {
		// I am the singleton ?
		//If yes then I should release the singleton as well
		if ( Instance == this ) {
			Instance = null;
		}
	}

}