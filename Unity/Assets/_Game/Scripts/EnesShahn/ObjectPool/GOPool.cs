using UnityEngine;

namespace EnesShahn.ObjectPool
{
    public class GOPool : GenericObjectPool<GameObject>
    {
        private GameObject poolContainer;

        public GOPool(GameObject pooledObject, int initialPoolSize, int growth) : base(pooledObject, initialPoolSize, growth) { }

        protected override void OnPoolInit()
        {
            poolContainer = new GameObject();
            poolContainer.name = pooledObject.name + " Pool";
        }
        protected override GameObject CreateObject()
        {
            GameObject obj = GameObject.Instantiate(pooledObject);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer.transform);
            return obj;
        }
        protected override void OnRequest(GameObject obj)
        {
            obj.SetActive(true);
        }
        protected override void OnReturn(GameObject obj)
        {
            obj.transform.SetParent(poolContainer.transform);
            obj.SetActive(false);
        }
    }
}