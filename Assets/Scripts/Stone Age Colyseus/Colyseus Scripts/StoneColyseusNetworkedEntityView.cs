using UnityEngine;
using System.Collections;

namespace Colyseus
{
    /// The base Networked Entity View
    public class StoneColyseusNetworkedEntityView : MonoBehaviour
    {
        /// The ID of the NetworkedEntity that belongs to this view.
        public string Id { get; protected set; }

    }
}
