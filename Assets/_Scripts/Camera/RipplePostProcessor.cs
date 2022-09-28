using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplePostProcessor : MonoBehaviour
{
    [SerializeField] private Material RippleMaterial;
    [SerializeField] private float MaxAmount = 50f;

    [Range(0, 1)]
    public float Friction = .9f;

    private float Amount = 0f;

    public static RipplePostProcessor Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de RipplePostProcessor dans la scène");
            return;
        }
        Instance = this;
    }

    void Update()
    {
        this.RippleMaterial.SetFloat("_Amount", this.Amount);
        this.Amount *= this.Friction;
    }

    public void RippleEffect(Vector3 pos)
    {
        this.Amount = this.MaxAmount;
        this.RippleMaterial.SetFloat("_CenterX", pos.x);
        this.RippleMaterial.SetFloat("_CenterY", pos.y);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, this.RippleMaterial);
    }
}