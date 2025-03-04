using UnityEngine;
using System.Collections;

public class ItemGrabScript : MonoBehaviour
{
    [SerializeField] private Transform rightHandTransform; // Превращение правой руки
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private GameObject throwButton;
    [SerializeField] private Transform characterCamera;
    private GameObject currentHeldItem;   // Текущий item
    private Rigidbody itemRb;

    void Update()
    {
        HandleTouchInput();
        UpdateHeldItemPosition();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0) // Проверяем, есть ли активный тап
        {
            foreach (Touch touch in Input.touches) // Проходим по всем активным тапам
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleGrab(touch);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Moved:
                        //HandleDrop();
                        break;
                }
            }
        }
    }

    private void HandleGrab(Touch touch)
    {
        // Создаем луч из камеры в точку taps
        Ray ray = Camera.main.ScreenPointToRay(touch.position);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Проверяем, что на месте удара находится GameObject с Rigidbody
            if (hit.collider.TryGetComponent<IItem>(out IItem item))
                if (hit.collider.attachedRigidbody && currentHeldItem == null)
                {
                    // Берем предмет в руку
                    currentHeldItem = hit.collider.gameObject;
                    currentHeldItem.transform.SetParent(rightHandTransform);
                    currentHeldItem.transform.localPosition = Vector3.zero;

                    itemRb = currentHeldItem.GetComponent<Rigidbody>();

                    // Останавливаем Rigidbody, чтобы предмет не двигался пока держится
                    itemRb.isKinematic = true;

                    throwButton.SetActive(true);
                }
        }
    }

    public Rigidbody HandleDrop()
    {
        if (currentHeldItem != null)
        {
            Rigidbody droppedItem;
            // Возвращаем Rigidbody в нормальное состояние
            currentHeldItem.transform.SetParent(null);
            itemRb.isKinematic = false;

            droppedItem = itemRb;
            // Освобождаем ссылку
            currentHeldItem = null;
            itemRb = null;

            throwButton.SetActive(false);

            return droppedItem;
        }
        return null;
    }

    public void Throw()
    {
        Rigidbody droppedItem = HandleDrop();
        droppedItem.AddForce(characterCamera.transform.forward * throwForce, ForceMode.Impulse);
    }

    private void UpdateHeldItemPosition()
    {
        if (currentHeldItem != null)
        {
            // Обновляем позицию item относительно правой руки
            currentHeldItem.transform.position = rightHandTransform.position;
            currentHeldItem.transform.rotation = rightHandTransform.rotation;
        }
    }
}