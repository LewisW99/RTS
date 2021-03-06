using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;


    private void Start()
    {
        
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>(); 
        }
        

        if(buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    //initially spawns the preview
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
       if(eventData.button !=PointerEventData.InputButton.Left) { return; }

       if(player.GetResources() < building.GetPrice()) { return; }


        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);
    }
    // if the preview is in a valid place,. it will then swap it for the proper building
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if(buildingPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    // if the preview is out of range it will go red, however if it can be placed it will go green
    void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }


        buildingPreviewInstance.transform.position = hit.point;

        if(!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;

        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }
}
