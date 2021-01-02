using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public GameObject Tooltip;

	public void OnPointerEnter(PointerEventData eventData)
	{
		Tooltip.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Tooltip.SetActive(false);
	}

	void Start()
	{
		Tooltip.SetActive(false);
	}
}
