using UnityEngine;
using UnityEngine.AI;
using System.Collections;


//credito de esto: LlamAcademy en youtube https://www.youtube.com/watch?v=dpJUc_BpChw


public class AgentLinkMover : MonoBehaviour
{

    [SerializeField] private NavMeshAgent _agent;
    private Quaternion _rotation, _rotationBuffer, _originalRotation;
    public bool jump = false;
    private Coroutine _cr;

    IEnumerator Start()
    {
        _agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (_agent.isOnOffMeshLink)
            {
                if (jump)
                {
                    yield return StartCoroutine(Parabola(_agent, 2.0f, 1f));
                    _agent.CompleteOffMeshLink();
                }
                else
                {
                    yield return StartCoroutine(NormalSpeed(_agent));
                    _agent.CompleteOffMeshLink();
                }
            }
            else
            {
                _originalRotation = transform.rotation;
            }
            yield return null;
        }
    }


    IEnumerator NormalSpeed(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;

        agent.updateRotation = false;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        while (agent.transform.position != endPos)
        {
            agent.transform.rotation = _originalRotation;
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            
            yield return null;
        }
        
        agent.updateRotation = true;
    }




    IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}