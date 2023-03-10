using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour {

	IEnumerator Start()
	{

		Debug.Log("First, check if user has location service enabled");
		if (!Input.location.isEnabledByUser)
		{
			Debug.Log("is Enabled By User false");
			yield break;
		}

		Debug.Log("Start service before querying location");
		Input.location.Start(500f,10f);

		Debug.Log("Wait until service initializes");
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		Debug.Log("Service didn't initialize in 20 seconds");
		if (maxWait < 1)
		{
			print("Timed out");
			yield break;
		}

		Debug.Log("Connection has failed");
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");
			yield break;
		}
		else
		{
			Debug.Log("Access granted and location value could be retrieved");
			print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
		}

		Debug.Log("Stop service if there is no need to query location updates continuously");
		Input.location.Stop();
	}
}
