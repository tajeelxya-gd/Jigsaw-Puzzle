using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI; // Required for RectTransform

public class PlayersListLeagueModelView : MonoBehaviour
{
    [SerializeField] private PlayerProfileModelView[] playerProfilesList;
    [SerializeField] ProfileAvatarsData profileAvatarsData;
    
    [Title("Snapping References")]
    [SerializeField] private PlayerProfileModelView snapperProfile;
    [SerializeField] private RectTransform viewport; // The visible area of the ScrollRect
    [SerializeField] private RectTransform scrollContent; // The moving container
    [SerializeField] private bool showRankedBadges = true;
    [ReadOnly, SerializeField]
    private RectTransform playerProfileReference;

    private ScrollRect scrollRectComponent = null;


    private void OnEnable()
    {
        if (playerProfileReference != null)
        {
            StartCoroutine(SnapScrollToPlayer());
        }
    }

    public void SetUp( List<LeaderboardData.PlayerData> leagueTop30)
    {
        if(scrollRectComponent == null)
            scrollRectComponent = scrollContent.GetComponent<ScrollRect>();
        DisableAll();
        for (int i = 0; i < leagueTop30.Count; i++)
        {
            LeaderboardData.PlayerData playerData = leagueTop30[i];
            playerProfilesList[i].gameObject.SetActive(true);
            playerProfilesList[i].Init(showRankedBadges,playerData.PlayerName, i, playerData.PlayerRank, playerData.Trophies, profileAvatarsData.GetProfileAvatar(playerData.PlayerAvatarID), playerData.IsPlayer);
            
            if (playerData.IsPlayer)
            {
                // Initialize the snapper with the same data
                snapperProfile.Init(showRankedBadges,playerData.PlayerName, i, playerData.PlayerRank, playerData.Trophies, profileAvatarsData.GetProfileAvatar(playerData.PlayerAvatarID), playerData.IsPlayer);
                playerProfileReference = playerProfilesList[i].GetComponent<RectTransform>();
            }
        }
    }
    private IEnumerator SnapScrollToPlayer()
    {
        // Wait for the end of the frame so UI Layout Groups can rebuild 
        // and calculate correct positions
        yield return new WaitForEndOfFrame();

        // Calculate the position of the player reference relative to the content
        // 1.0 is Top, 0.0 is Bottom
        Canvas.ForceUpdateCanvases();

        float contentHeight = scrollContent.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight > viewportHeight)
        {
            // Get the local position of the player element within the content
            float playerY = playerProfileReference.localPosition.y;
            
            // Calculate the normalized position (0 to 1)
            // We want to center the player, so we subtract half the viewport height
            float targetScrollPos = 1f - (Mathf.Abs(playerY) - (viewportHeight / 2f)) / (contentHeight - viewportHeight);
            
            scrollRectComponent.verticalNormalizedPosition = Mathf.Clamp01(targetScrollPos);
        }
    }
    private void Update()
    {
        
        if (playerProfileReference != null && snapperProfile != null)
        {
            UpdateSnapperPosition();
        }
    }

    private void UpdateSnapperPosition()
    {
        // Get the corners of the viewport in world space
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);
        float topEdge = viewportCorners[1].y;    // Top-left y
        float bottomEdge = viewportCorners[0].y; // Bottom-left y

        // Get the corners of the actual player profile in world space
        Vector3[] profileCorners = new Vector3[4];
        playerProfileReference.GetWorldCorners(profileCorners);
        float profileTop = profileCorners[1].y;
        float profileBottom = profileCorners[0].y;

        // Logic for Sticky Snapping
        if (profileTop > topEdge)
        {
            // The profile is hidden ABOVE the top edge
            SetSnapperActive(true);
            SnapToEdge(topEdge, true);
        }
        else if (profileBottom < bottomEdge)
        {
            // The profile is hidden BELOW the bottom edge
            SetSnapperActive(true);
            SnapToEdge(bottomEdge, false);
        }
        else
        {
            // The profile is visible inside the scroll area
            SetSnapperActive(false);
        }
    }

    private void SnapToEdge(float worldY, bool isTop)
    {
        Vector3 currentPos = snapperProfile.transform.position;
        // Keep horizontal position, snap vertical
        snapperProfile.transform.position = new Vector3(currentPos.x, worldY, currentPos.z);
        
        // Pivot adjustment: Ensure the snapper sits INSIDE the edge
        RectTransform rt = snapperProfile.GetComponent<RectTransform>();
        if (isTop) rt.pivot = new Vector2(0.5f, 1f); // Top-center
        else rt.pivot = new Vector2(0.5f, 0f);       // Bottom-center
    }

    private void SetSnapperActive(bool active)
    {
        if (snapperProfile.gameObject.activeSelf != active)
            snapperProfile.gameObject.SetActive(active);

//        if (playerProfileReference.gameObject.activeSelf == active)
        //    playerProfileReference.gameObject.SetActive(!active);
    }

    void DisableAll()
    {
        foreach (var playerProfile in playerProfilesList)
            playerProfile.gameObject.SetActive(false);
    }
}