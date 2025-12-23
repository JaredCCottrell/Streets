using UnityEngine;

namespace Streets.Dialogue
{
    /// <summary>
    /// Makes the GameObject always face the camera (billboard effect).
    /// Useful for world-space UI prompts.
    /// </summary>
    public class BillboardPrompt : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.LookAt(transform.position + cam.transform.forward);
            }
        }
    }
}
