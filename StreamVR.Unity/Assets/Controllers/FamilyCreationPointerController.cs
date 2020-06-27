﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace LMAStudio.StreamVR.Unity.Scripts
{
    public class FamilyCreationPointerController : MonoBehaviour
    {
        public GameObject physicsPointer;

        public float defaultLength = 3.0f;

        private LineRenderer lineRenderer = null;

        private bool colliderHit = false;

        private Common.Models.Family familyDef;
        private GameObject familyToCreate;
        private Quaternion initialRotation;

        public void SpawnFamily(string familyName)
        {
            Debug.Log("Spawning family: " + familyName);

            familyDef = Logic.FamilylLibrary.ReverseGetFamily(familyName);

            GameObject model = (GameObject)Resources.Load($"Families/{familyName}/model");
            familyToCreate = Instantiate(model);
            familyToCreate.transform.parent = this.transform;
            familyToCreate.transform.position = this.transform.position + (1.5f * this.transform.forward);
            initialRotation = familyToCreate.transform.rotation;
        }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            if (familyToCreate != null)
            {
                this.GetComponent<LineRenderer>().enabled = true;

                Vector3 collisionPoint = CalculatedEnd();

                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, collisionPoint);

                familyToCreate.transform.position = collisionPoint;
                familyToCreate.transform.rotation = initialRotation;

                if (colliderHit)
                {
                    familyToCreate.SetActive(true);

                    InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

                    bool clicked;
                    device.TryGetFeatureValue(CommonUsages.triggerButton, out clicked);

                    if (clicked)
                    {
                        PlaceObject();
                    }
                }
                else
                {
                    familyToCreate.SetActive(false);
                }
            }
            else
            {
                this.GetComponent<LineRenderer>().enabled = false;
            }
        }

        private void PlaceObject()
        {
            Debug.Log("PLACING OBJECT!");

            GameObject container = new GameObject();

            container.transform.position = familyToCreate.transform.position;
            familyToCreate.transform.parent = container.transform;
            familyToCreate.transform.localPosition = Vector3.zero;

            FamilyController controller = container.AddComponent<FamilyController>();
            controller.PlaceFamily(familyDef.Id);

            familyDef = null;
            familyToCreate = null;
            initialRotation = Quaternion.identity;

            physicsPointer.SetActive(true);
            this.gameObject.SetActive(false);

            Debug.Log("OBJECT PLACED!");
        }

        private Vector3 CalculatedEnd()
        {
            RaycastHit hit = CreateForwardRaycast();

            colliderHit = hit.collider;

            if (hit.collider)
            {
                return hit.point;
            }

            return DefaultEnd(defaultLength);
        }

        private RaycastHit CreateForwardRaycast()
        {
            RaycastHit hit;

            Ray ray = new Ray(transform.position, transform.forward);

            Physics.Raycast(ray, out hit, defaultLength, 1 << 11);

            return hit;
        }

        private Vector3 DefaultEnd(float length)
        {
            return transform.position + (transform.forward * length);
        }
    }
}