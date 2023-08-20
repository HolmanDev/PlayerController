# PlayerController
Third and first-person player controller with support for climbing, sliding, moving platforms and more.
#
#### NOTE FROM THE FUTURE
Getting weird behaviour when jumping off slopes? Try adding the following near the top of the Update() method in PlayerController.cs
``` csharp
if (!IsGrounded)
  SlopeNormal = Vector3.up;
```
