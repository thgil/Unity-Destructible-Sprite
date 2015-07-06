#Destructible sprites with polygon colliders
<img src="http://puu.sh/iOKvt/38b454e6c1.png" width="150"/>
<img src="http://puu.sh/iOKx2/a237d1c7f6.png" width="150"/>
##Usage
Attach `DestructibleSprite.cs` as a script component to the sprite.

When a projectile hits the sprites polygon collider call `ApplyDamage(pos, radius)` at the hit point.

 `colliders[i].GetComponent<DestructibleSprite>().ApplyDamage(explosionPos, explosionRadius)`

##Generating a polygon collider for a sprite at runtime
We start by generating a binary image(b) from the texture.
<img src="http://puu.sh/iOKjr/8231dfaf90.png" width="150"/>

We can preform two helpful functions on this binary image.

<img src="http://puu.sh/iOKjX/09d3b2f80b.png" width="150"/> Erosion (E) - shrinking of the image

<img src="http://puu.sh/iOKkh/2a786539bf.png" width="150"/> Dilation (D) - boarding of image

We can clean up our binary image by appply these functions.
b<sub>0</sub>= D(E(b))

We can get the outline of the image b<sub>s</sub> = b<sub>0</sub> - E(b<sub>0</sub>)

<img src="http://puu.sh/iOKla/636a84adc0.png" width="150"/> Subtraction (S)

We can add each pixel now as a vertex to the polygon collider. Preforming some simplification we get a simple path the collider can use.

<img src="http://puu.sh/iOKlO/77f2525fdc.png" width="150"/> Each point is a vertex on the collider

<img src="http://puu.sh/iOKvt/38b454e6c1.png" width="150"/> <img src="http://puu.sh/iOKx2/a237d1c7f6.png" width="150"/>
