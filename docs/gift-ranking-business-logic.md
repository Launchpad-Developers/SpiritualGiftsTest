# Gift Ranking Business Logic Specification

**Document Version:** 1.0  
**Last Updated:** 2026-05-29  
**Author:** System Documentation  
**Status:** ✅ Implemented and Tested

---

## Overview

This document defines the **business logic** for ranking spiritual gifts based on user survey responses. The ranking algorithm determines which gifts receive **Primary** (Gold medal 🥇), **Secondary** (Silver medal 🥈), or **No** medal based on calculated scores.

---

## Business Requirements

### Core Rules

1. **Primary Gifts (Gold Medal 🥇)**
   - Top 3 highest-scoring gifts **at minimum**
   - If tied scores, **all tied gifts** are included (can exceed 3)
   - Represents the user's **strongest spiritual gifts**

2. **Secondary Gifts (Silver Medal 🥈)**
   - Next tier of gifts after Primary
   - Combined total (Primary + Secondary) should aim for **≤ 6 gifts**
   - If tied scores, **all tied gifts** are included (can exceed 6)
   - Represents gifts the user is **developing or has moderate strength in**

3. **No Medal (None Rank)**
   - All remaining gifts outside top 6 slots
   - Represents gifts that are **not currently prominent**

### Tie Handling

**Critical Rule:** When multiple gifts have the same score, they **all receive the same rank**.

**Example:**
- If 4 gifts tie for the top score → All 4 get Primary (Gold)
- If 3 gifts tie for positions 4-6 → All 3 get Secondary (Silver)

This ensures **fairness** - users see all their top-scoring gifts recognized equally.

---

## Score Calculation

### User Response Values

Each question allows 4 responses with point values:

| Response | Points | Meaning |
|----------|--------|---------|
| **Much** | 3 | Statement is very true for me |
| **Some** | 2 | Statement is somewhat true for me |
| **Little** | 1 | Statement is minimally true for me |
| **Not at All** | 0 | Statement is not true for me |

### Gift Score Formula

```
Gift Score = Sum of all question responses for that gift
Max Score = Number of questions × 3
```

**Example:**
- Gift: "Teaching"
- Questions: 10
- User answered: 5×Much, 3×Some, 2×Little, 0×NotAtAll
- **Score** = (5×3) + (3×2) + (2×1) + (0×0) = 15 + 6 + 2 = **23**
- **Max Score** = 10 × 3 = **30**
- **Percentage** = 23/30 = **76.7%**

---

## Ranking Algorithm

### High-Level Flow

```
1. Group all gifts by score (descending)
2. Assign Primary rank (top 3+ with ties)
3. Assign Secondary rank (next gifts up to 6 total, with ties)
4. Assign None rank to remaining gifts
```

### Detailed Algorithm

**Step 1: Group by Score**

```csharp
var scoreGroups = Scores
    .OrderByDescending(x => x.Score)
    .GroupBy(x => x.Score)
    .OrderByDescending(g => g.Key)
    .ToList();
```

Result: Groups of gifts with same score, ordered from highest to lowest.

**Example:**
```
Group 1: Score 50 → [Prophecy, Teaching]
Group 2: Score 45 → [Wisdom]
Group 3: Score 30 → [Faith, Mercy, Giving]
Group 4: Score 20 → [Leadership]
```

---

**Step 2: Assign Primary (Gold) Medals**

Rules:
- Take the **first score group** (highest score)
- Keep adding score groups until we have **at least 3 gifts**
- Stop when we reach or exceed 3 gifts

```csharp
var primary = new List<UserGiftScore>();

foreach (var group in scoreGroups) {
    if (primary.Count == 0) {
        primary.AddRange(group);  // Always add first group
    } else if (primary.Count < 3) {
        primary.AddRange(group);  // Keep adding until 3+
    } else {
        break;  // We have 3+, stop
    }
}
```

**Example 1: Clear Top 3**
```
Scores: [50, 48, 45, 30, 28]
Group 1: 50 (1 gift)   → Add → primary.Count = 1
Group 2: 48 (1 gift)   → Add → primary.Count = 2
Group 3: 45 (1 gift)   → Add → primary.Count = 3
Group 4: 30 (1 gift)   → STOP (already have 3)

Result: 3 Primary gifts
```

**Example 2: Tied at Top**
```
Scores: [50, 50, 50, 30, 28]
Group 1: 50 (3 gifts)  → Add → primary.Count = 3
Group 2: 30 (1 gift)   → STOP (already have 3)

Result: 3 Primary gifts (all tied)
```

**Example 3: Tie Exceeds 3**
```
Scores: [50, 50, 48, 48, 30]
Group 1: 50 (2 gifts)  → Add → primary.Count = 2
Group 2: 48 (2 gifts)  → Add → primary.Count = 4
Group 3: 30 (1 gift)   → STOP (already have 4 > 3)

Result: 4 Primary gifts (ties included)
```

---

**Step 3: Assign Secondary (Silver) Medals**

Rules:
- Skip groups already in Primary
- Add groups until **total slots (Primary + Secondary) ≥ 6**
- If adding a group would exceed 6, **still add it** (to honor ties)

```csharp
var secondary = new List<UserGiftScore>();
int totalSlots = primary.Count;

foreach (var group in scoreGroups.SkipWhile(g => primary.Contains(g.First()))) {
    if (totalSlots >= 6) break;  // Already at 6+
    
    if (totalSlots + group.Count() <= 6) {
        // Fits within 6
        secondary.AddRange(group);
        totalSlots += group.Count();
    } else {
        // Would exceed 6, but add anyway (honor ties)
        secondary.AddRange(group);
        break;
    }
}
```

**Example 1: Fits Exactly**
```
Primary: 3 gifts (scores 50, 48, 45)
Remaining: [30, 28, 26, 20]

Group 4: 30 (1 gift) → totalSlots = 4 → Add
Group 5: 28 (1 gift) → totalSlots = 5 → Add
Group 6: 26 (1 gift) → totalSlots = 6 → Add, STOP

Result: 3 Primary + 3 Secondary = 6 total
```

**Example 2: Tie Exceeds 6**
```
Primary: 3 gifts (scores 50, 48, 45)
Remaining: [30, 30, 30, 20]

Group 4: 30 (3 gifts) → totalSlots would be 6 → Add all 3, STOP

Result: 3 Primary + 3 Secondary = 6 total
```

**Example 3: Tie Pushes Over 6**
```
Primary: 4 gifts (scores 50, 50, 48, 48)
Remaining: [30, 30, 30, 20]

Group 5: 30 (3 gifts) → totalSlots would be 7 > 6 → Add anyway (honor tie), STOP

Result: 4 Primary + 3 Secondary = 7 total
```

---

**Step 4: Assign Ranks**

```csharp
foreach (var score in Scores) {
    if (primary.Contains(score))
        score.GiftRank = GiftRank.Primary;
    else if (secondary.Contains(score))
        score.GiftRank = GiftRank.Secondary;
    else
        score.GiftRank = GiftRank.None;
}
```

---

## Edge Cases & Scenarios

### Scenario Matrix

| Scenario | Primary Count | Secondary Count | Total with Medal | Notes |
|----------|--------------|----------------|------------------|-------|
| **Clear Top 3** | 3 | 3 | 6 | Standard case |
| **3-way tie at top** | 3 | 3 | 6 | All tied get same rank |
| **4-way tie at top** | 4 | 2 | 6 | Ties honored |
| **5-way tie at top** | 5 | 1 | 6 | Ties honored |
| **2 scores, tied top 3** | 3 | 3 | 6 | Top score = 3 gifts tied |
| **Tie pushes over 6** | 4 | 3 | 7 | Exceeds 6 to honor ties |
| **Many ties** | 5 | 4 | 9 | Multiple tie groups |
| **All same score** | All | 0 | All | Everyone gets Primary |
| **Only 1 gift** | 1 | 0 | 1 | Edge case |
| **Empty list** | 0 | 0 | 0 | No ranking performed |

---

## Test Coverage

### Unit Tests

**File:** `SpiritualGiftsSurvey.Tests/SurveyResultRankingTests.cs`  
**Total Tests:** 8  
**Status:** ✅ All Passing

| Test | Purpose | Edge Case Coverage |
|------|---------|-------------------|
| `RankGiftsAsync_AssignsPrimaryGifts_ForTopScores` | Basic Primary assignment | Standard 3 gifts |
| `RankGiftsAsync_AssignsSecondaryGifts_ForMiddleScores` | Secondary assignment | 3+3 pattern |
| `RankGiftsAsync_HandlesTies_IncludesAllTiedGiftsInSameRank` | Tie handling | 3-way tie at top |
| `RankGiftsAsync_DoesNotCrash_WhenScoresIsEmpty` | Empty collection | ✅ Edge case |
| `RankGiftsAsync_DoesNotCrash_WhenScoresIsNull` | Null safety | ✅ Edge case |
| `RankGiftsAsync_IsIdempotent_MultipleCalls` | Idempotency | ✅ Consistency |
| `RankGiftsAsync_ConcurrentCalls_DoNotCorruptState` | Thread safety | ✅ Concurrency |
| `RankGiftsAsync_AssignsNoRank_ForLowestScores` | None rank assignment | 9 gifts, bottom 2 get None |

---

### Additional Edge Cases NOT YET TESTED ⚠️

While current tests cover **most** scenarios, these edge cases could be added:

1. **Single Gift Only**
   - Scores: `[50]`
   - Expected: 1 Primary, 0 Secondary
   - **Current Coverage:** ❌ Not explicitly tested

2. **Two Gifts Total**
   - Scores: `[50, 30]`
   - Expected: 2 Primary (need at least 3, so take what we have)
   - **Current Coverage:** ❌ Not explicitly tested
   - **Question:** Should we require minimum 3 Primary, or adapt to available gifts?

3. **Exactly 6 Gifts, All Different Scores**
   - Scores: `[50, 45, 40, 35, 30, 25]`
   - Expected: 3 Primary, 3 Secondary
   - **Current Coverage:** ✅ Covered by basic tests

4. **Tie Creates Exactly 6 Primary**
   - Scores: `[50, 50, 50, 48, 48, 48, 20]`
   - Expected: 6 Primary (2 tie groups), 0 Secondary
   - **Current Coverage:** ❌ Not explicitly tested

5. **All Gifts Tied at Same Score**
   - Scores: `[30, 30, 30, 30, 30, 30]` (all 6 gifts)
   - Expected: All 6 Primary
   - **Current Coverage:** ❌ Not explicitly tested
   - **Behavior:** First group added → all get Primary → loop stops

6. **Tie at Secondary Boundary Exceeds 10 Total**
   - Scores: `[50, 48, 45, 30, 30, 30, 30, 30, 20]`
   - Primary: 3 (scores 50, 48, 45)
   - Secondary: Would be 5 gifts tied at 30
   - Total: 8 gifts with medals
   - **Current Coverage:** ❌ Not explicitly tested

---

## Performance Characteristics

- **Time Complexity:** O(n log n) where n = number of unique gifts (typically 20-30)
  - Dominated by sorting operations
  - GroupBy + OrderBy = O(n log n)

- **Space Complexity:** O(n)
  - Stores score groups, primary list, secondary list

- **Typical Performance:** < 1ms for 28 gifts on modern devices

---

## Implementation Details

### Thread Safety

**Issue (Historical):** Original implementation used `Task.Run()` to mutate shared `Scores` collection on background thread without synchronization.

**Resolution (HIGH-5 Fix):** Added `SemaphoreSlim` to serialize concurrent ranking calls:

```csharp
private readonly SemaphoreSlim _rankLock = new(1, 1);
private Task? _rankingTask;

public async Task RankGiftsAsync()
{
    // If already ranked, skip
    if (IsRanked) return;
    
    // If ranking in progress, await it
    if (_rankingTask != null && !_rankingTask.IsCompleted) {
        await _rankingTask;
        return;
    }
    
    // Acquire lock
    await _rankLock.WaitAsync();
    try {
        _rankingTask = Task.Run(() => { /* ranking logic */ });
        await _rankingTask;
        IsRanked = true;
    } finally {
        _rankLock.Release();
    }
}
```

**Test:** `RankGiftsAsync_ConcurrentCalls_DoNotCorruptState` - Fires 10 concurrent calls, verifies consistent results.

---

### Idempotency

**Requirement:** Calling `RankGiftsAsync()` multiple times should produce identical results.

**Implementation:**
- `IsRanked` flag prevents re-ranking
- Early return if already ranked
- No side effects on subsequent calls

**Test:** `RankGiftsAsync_IsIdempotent_MultipleCalls` - Calls twice, compares results.

---

## Business Logic Validation Checklist

When modifying the ranking algorithm, verify:

- [ ] Top 3 gifts (minimum) receive Primary rank
- [ ] Tied scores at same rank boundary are **all** included
- [ ] Total aims for ≤ 6 but can exceed for ties
- [ ] Gifts outside top 6 slots receive None rank
- [ ] Empty/null collections handled gracefully
- [ ] Concurrent calls produce consistent results
- [ ] Idempotent (calling multiple times is safe)
- [ ] All 8 unit tests pass

---

## Future Considerations

### Potential Enhancements

1. **Configurable Thresholds**
   - Make "3 primary" and "6 total" configurable via Firebase
   - Allows business logic changes without code deployment

2. **Minimum Score Threshold**
   - Only award medals if score > X% of MaxScore
   - Prevents low-scoring gifts from getting medals

3. **Score Normalization**
   - If gifts have different numbers of questions, normalize scores
   - Current: Gift with 10 questions maxes at 30, gift with 5 questions maxes at 15
   - Enhancement: Calculate percentage and rank by percentage instead

4. **Tertiary Rank**
   - Add Bronze medal for positions 7-9
   - Requires UI changes + logic updates

---

## Related Documentation

- **Implementation:** `Models/SurveyResult.cs` (lines 38-132)
- **Test Suite:** `SpiritualGiftsSurvey.Tests/SurveyResultRankingTests.cs`
- **Architecture:** `docs/architecture-overview.md` (Ranking Algorithm section)
- **Thread Safety:** `docs/async-and-threading-guidelines.md`
- **Technical Debt:** `docs/technical-debt-register.md` (TD-008 - Ranking thread safety)

---

## Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-05-29 | 1.0 | Initial business logic specification | System Documentation |

---

## Approval

- [x] Business Logic Documented
- [x] Test Coverage Assessed
- [x] Edge Cases Identified
- [ ] Product Owner Review (Pending)
- [ ] Additional Edge Case Tests (Recommended)

