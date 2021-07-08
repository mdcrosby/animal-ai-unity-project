# DecayGoal docs and defaults

## *Default colours*
For green -> yellow -> red decay that includes episode-ending at extreme green/red, the following HDR colour values should be used:

**Good Colour:** (HDR) (129, 191, 65, 255, intensity=0)
**Neutral Colour:** (HDR) (232, 185, 35, 255, intensity=0)
**Bad Colour:** (HDR) (159, 25, 25, 255, intensity=0)

These have been included here because they have to be assigned in editor (Unity scripts can't handle HDR colour assignments well at all).

## *Using neutral colour / episode-ending thresholds*

To use the neutral reward/colour in colour interpolation, toggle **Use Middle**.

To use the thresholds for good/bad reward values ending an episode, toggle **Use Good/Bad Episode End Threshold**.

If you want to ignore the use of a middle colour, or treat the goal as a multi-goal in all cases, just set the above three parameters to false.
