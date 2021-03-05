using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Article
{
    public class Walker : MonoBehaviour
    {
        public float walkSpeed = 4.0f;
        public int index = 0;

        private Vector3 walkFrom = Vector3.zero;
        private Vector3 walkVector = Vector3.zero;
        private List<Vector3> walkDirectionQueue = new List<Vector3>();

        private void Update()
        {
            this.UpdateWalkPosition(Time.deltaTime);

            this.UpdateWalkVector();
        }

        public bool IsWalking()
        {
            return this.walkVector != Vector3.zero;
        }

        public void AppendWalkDirections(List<Vector3> directions)
        {
            this.walkDirectionQueue.AddRange(directions);
        }

        private void UpdateWalkPosition(float dt)
        {
            if (!this.IsWalking())
                return;

            this.transform.position += this.walkVector * this.walkSpeed * dt;
            Vector3 dest = this.walkFrom + this.walkVector;
            float estimatedDistance = Vector3.Distance(this.walkFrom, dest);
            float totalDistance = Vector3.Distance(this.walkFrom, this.transform.position);
            if (totalDistance >= estimatedDistance)
            {
                this.transform.position = this.walkFrom + this.walkVector;
                this.walkVector = Vector3.zero;
            }
        }

        private void UpdateWalkVector()
        {
            if (this.walkDirectionQueue.Count == 0)
                return;

            if (this.IsWalking())
                return;

            this.walkFrom = this.transform.position;

            Vector3 nextDirection = this.walkDirectionQueue[0];
            this.walkDirectionQueue.RemoveAt(0);
            this.walkVector = nextDirection;
        }
    }
}