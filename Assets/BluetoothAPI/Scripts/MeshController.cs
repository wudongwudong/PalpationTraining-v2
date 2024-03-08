using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;




public class MeshController : MonoBehaviour
{
    //IMixedRealityDataProviderAccess dataProviderAccess = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
    //private void Start()
    //{
    //    hide_mesh();
    //}
    private void Start()
    {
        hide_mesh();
    }
    public void hide_mesh()
    {
        //if (dataProviderAccess != null)
        //{
        //    IReadOnlyList<IMixedRealitySpatialAwarenessMeshObserver> observers = dataProviderAccess.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
        //    foreach (IMixedRealitySpatialAwarenessMeshObserver observer in observers)
        //    {

        //        // 设置网格使用遮挡材质
        //        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        //    }

        //}
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        // Set to not visible
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;

        
    }

    public void show_mesh()
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        // Set to visible and the Occlusion material
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
    }
}
