using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

[RequireComponent (typeof (PolygonCollider2D))]
public class DestructibleSprite : MonoBehaviour {

	public Texture2D tex;

	// Testing
	public bool doInit = true;
	public bool doSplit = false;

	public bool doRefresh;
	public bool doBI;
	public bool doErosion;
	public bool doDilation;
	public bool doSub;
	public bool doSub2;
	public bool doVert;
	public bool doVertLong;
	public bool doComplete;

	public float pixelsToUnits = 100f; // Pixels to unity units  100 to 1
	public float pixelOffset = 0.5f;

	private BinaryImage binaryImage;

	PolygonCollider2D poly;

	public float xBounds;
	public float yBounds;

	public int islandCount=0;

	private List<Vector2> tempPath;
	private Vector2 endPoint;

	// Use this for initialization
	void Start () {

		poly = gameObject.GetComponent<PolygonCollider2D>();
		tex = Instantiate(gameObject.GetComponent<SpriteRenderer>().sprite.texture) as Texture2D;

		xBounds = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
		yBounds = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.extents.y;

		if(doInit) {
			binaryImage = BinaryImageFromTex(ref tex);
			binaryImage = tidyBinaryImage(binaryImage);
			updateCollider();
		}
	}

	void Update() {

		if(doRefresh) {
			doRefresh = false;
			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doBI) {
			doBI=false;

			binaryImage = BinaryImageFromTex(ref tex);

			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(BinaryImage2Texture(binaryImage), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doErosion) {
			doErosion=false;

			binaryImage = BinaryImageFromTex(ref tex);

			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(BinaryImage2Texture(erosion(binaryImage)), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doDilation) {
			doDilation=false;

			binaryImage = BinaryImageFromTex(ref tex);

			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(BinaryImage2Texture(dilation(binaryImage)), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doSub) {
			doSub=false;

			binaryImage = BinaryImageFromTex(ref tex);
			binaryImage = tidyBinaryImage(binaryImage);

			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(BinaryImage2Texture(subtraction(binaryImage, erosion(binaryImage))), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doSub2) {
			doSub2=false;

			binaryImage = BinaryImageFromTex(ref tex);
			binaryImage = tidyBinaryImage(binaryImage);

			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(BinaryImage2Texture(subtraction(binaryImage, erosion(binaryImage))), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doVert) {
			doVert=false;

			binaryImage = BinaryImageFromTex(ref tex);
			binaryImage = tidyBinaryImage(binaryImage);
			BinaryImage binaryImageOutline = subtraction(binaryImage, erosion(binaryImage));
			List<List<Vector2> > paths = getPaths(ref binaryImageOutline);
			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create( BinaryImage2TextureUsingPaths(binaryImageOutline, paths), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));

			setCollider(ref paths);
		}

		if(doVertLong) {
			doVertLong=false;

			// TODO: simplify paths farther
			print("doVertLong has not been made yet :3");

			gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(BinaryImage2Texture(binaryImage), gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));
		}

		if(doComplete) {
			doComplete=false;

			binaryImage = BinaryImageFromTex(ref tex);
			binaryImage = tidyBinaryImage(binaryImage);
			updateCollider();
		}
	}

	void split(BinaryImage b) {
		int startPos;

		// TODO: copy the binaryImage instead of setting it
		BinaryImage t = new BinaryImage(b.x, b.y);
		for(int x=0; x<b.Length; x++) t.Set(x, b.Get(x));

		List<List<int> > islands = new List<List<int> >();

		// Find islands
		while(findStartPos(ref t, out startPos)) {
			List<int> island = new List<int>();

			floodFill(ref t, ref island, startPos);

			islands.Add(island);
		}

		// If there is only 1 island we wont split anything
		if(islands.Count <= 1) return;

		// Get bounding boxes for each island
		for(int i=0; i<islands.Count; i++) {
			int x1, y1, x2, y2;
			x1 = x2 = islands[i][0]%b.x;
			y1 = y2 = Mathf.FloorToInt((float)islands[i][0]/b.x);

			// Find the smallest and biggest points
			for(int j=0; j<islands[i].Count; j++) {
				int x = islands[i][j]%b.x, y = Mathf.FloorToInt((float)islands[i][j]/b.x);
				if(x < x1) x1 = x;
				else if(x > x2) x2 = x;
				if(y < y1) y1 = y;
				else if(y > y2) y2 = y;
			}

			int w = x2-x1, h = y2-y1; // bounds
			int cx = (x2+x1)/2, cy = (y2+y1)/2; // new center for island

			// Create new gameobject
			GameObject go = new GameObject("DestructibleSpritePiece");
			go.AddComponent<SpriteRenderer>();
			go.AddComponent<Rigidbody2D>();
			go.AddComponent<DestructibleSprite>();
			go.GetComponent<DestructibleSprite>().doSplit = true;

			// Copy part of the original texture to our new texture
			Color32[] d = tex.GetPixels32();
			Color32[] e = new Color32[w*h];
			for(int x=0, y=0; x<d.Length; x++) {
				if(x%tex.width>=x1 && x%tex.width<x2 && Mathf.FloorToInt((float)x/tex.width)<y2 && Mathf.FloorToInt((float)x/tex.width)>=y1) {
					e[y] = d[x];
					y++;
				}
			}

			// Apply to our new texture
			Texture2D texture = new Texture2D(w,h);
			texture.SetPixels32(e);
			texture.Apply();

			// Add the spriteRenderer and apply the texture and inherit parent options
			SpriteRenderer s = go.GetComponent<SpriteRenderer>();
			s.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			s.color = gameObject.GetComponent<SpriteRenderer>().color;
			s.sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder;

			// Set the position to the new center
			go.transform.position = new Vector3(transform.position.x + (cx + pixelOffset)/pixelsToUnits - xBounds, transform.position.y + (cy + pixelOffset)/pixelsToUnits - yBounds, transform.position.z);
			// Put it in the same layer as the parent
			go.layer = gameObject.layer;

		}
		// We can destroy the orignal object
		Destroy(gameObject);
	}

	void floodFill(ref BinaryImage b, ref List<int> i, int pos) {
		int w = b.x;

		if(b.Get(pos)) {
			i.Add(pos);
			b.Set(pos, false);
		}	else return;

		if((pos%w)+1 < w) floodFill(ref b, ref i, pos+1);				// Right
		if((pos%w)-1 >= 0) floodFill(ref b, ref i, pos-1);			// Left
		if(pos+w < b.Length) floodFill(ref b, ref i, pos+w);		// Top
		if(pos-w >= 0) floodFill(ref b, ref i, pos-w);					// Bottom
	}

	/**
	 * Remove a part of the texture
	 * @param  {Vector2} point         World point
	 * @param  {int}     radius        Radius of destroyed area
	 */
	public void ApplyDamage(Vector2 point, int radius) {

		// edit binaryImage
		int w = binaryImage.x, h = binaryImage.y;

		// get relative position of the circle
		Vector2 rPos = new Vector2(point.x - transform.position.x, point.y - transform.position.y);

		// get rotation matrix
		float theta = transform.rotation.eulerAngles.z * Mathf.PI / 180f;
		float sin = Mathf.Sin(theta);
		float cos = Mathf.Cos(theta);

		// apply rotation to the circle centre
		Vector2 c = new Vector2(rPos.x*cos + rPos.y*sin, -rPos.x*sin + rPos.y*cos);

		c.x = (xBounds + (c.x / transform.localScale.x))/(xBounds*2) * w;
		c.y = (yBounds + (c.y / transform.localScale.y))/(yBounds*2) * h;

		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				float dx = x-c.x;
				float dy = y-c.y;
				float dist = Mathf.Sqrt(dx*dx+dy*dy);
				if(dist <= radius) {
					binaryImage.Set(x + y*w, false);
				}
			}
		}

		updateCollider();
	}

	// TODO: make into a coroutine?/separate thread
	private void updateCollider() {
		tex = ApplyBinaryImage2Texture(ref tex, ref binaryImage);
		if(doSplit) split(binaryImage);

		// binaryImage = tidyBinaryImage(binaryImage);
		BinaryImage binaryImageOutline = subtraction(binaryImage, erosion(binaryImage));
		List<List<Vector2> > paths = getPaths(ref binaryImageOutline);

		gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, gameObject.GetComponent<SpriteRenderer>().sprite.rect, new Vector2(0.5f, 0.5f));

		setCollider(ref paths);
	}

	private void setCollider(ref List<List<Vector2> > paths) {
		poly.pathCount = paths.Count;
		islandCount = paths.Count;

		for(int i=0; i<paths.Count; i++) {
			for(int j=0; j<paths[i].Count; j++) {
				paths[i][j] = new Vector2((paths[i][j].x + pixelOffset)/pixelsToUnits - xBounds, (paths[i][j].y + pixelOffset)/pixelsToUnits - yBounds);
			}
			poly.SetPath( i, paths[i].ToArray() );
		}
	}

	// returns true if found a start point
	private bool findStartPos(ref BinaryImage b, out int startPos) {
		int w = b.x, h = b.y;

		for(int x= 0; x<w; x++){
			for(int y= 0; y<h; y++){
				if(b.Get(x + y*w)) {
					startPos = x + y*w;
					return true;
				}
			}
		}

		startPos = 0;
		return false; // Cannot find any start points.
	}

	private List<List<Vector2> > getPaths(ref BinaryImage b) {
		int startPos;
		List<List<Vector2> > paths = new List<List<Vector2> >();

		while(findStartPos(ref b, out startPos)) {
			List<Vector2> path = new List<Vector2>();

			// Find path
			path = getPath(ref b, startPos);
			if(path != null) paths.Add(path);
		}

		for(int i=0; i<paths.Count; i++) paths[i] = simplify(ref b, paths[i]);

		return paths;
	}

	private List<Vector2> getPath(ref BinaryImage b, int startPos) {
		List<Vector2> path = new List<Vector2>();

		// Add start point to path
		path.Add(new Vector2(startPos%b.x, startPos/b.x));

		int pos = 0, prevPos = startPos, currPos = startPos;
		bool open = true;

		if(!nextPos(ref b, ref pos, prevPos)) {
			// No other points found from the starting point means this is a single pixel island so we can remove it
			b.Set(currPos, false);
			return null;
		}

		// While there is a next pos
		while(open) {
			if(nextPos(ref b, ref pos, ref currPos, ref prevPos)) {
				// b.Set(pos, false);
				if(currPos == startPos) open = false; // We found a closed path
				path.Add(new Vector2(currPos%b.x, currPos/b.x));
				b.Set(currPos, false);
			} else {
				// If no next position, backtrack till we find a closed path
				var index = backTrack(ref b, ref path, path.Count-1);

				if(index!=-1) {
					// find next new point!
					path.RemoveRange(index+1, path.Count-1-index);

					pos = (int)path[index].x + (int)path[index].y*b.x;
					prevPos = (int)path[index-1].x + (int)path[index-1].y*b.x;
					currPos = pos;

				} else open = false; // If we cannot, close the path (this is the worst case and will give us a buggy collider)
			}
		}

		return path;
	}

	private int backTrack(ref BinaryImage b, ref List<Vector2> path, int start) {
		int w = b.x;

		if(start <= 1) return -1;
		int currPos = (int)path[start].x + (int)path[start].y*w;
		int prevPos = (int)path[start-1].x + (int)path[start-1].y*w;

		if(currPos+w < b.Length && b.Get(currPos+w) && (currPos+w)!=prevPos	||		// Top
			(currPos%w)+1 < w 		&& b.Get(currPos+1) && (currPos+1)!=prevPos	||		// Right
			currPos-w >= 0 	 			&& b.Get(currPos-w) && (currPos-w)!=prevPos	||		// Bottom
			(currPos%w)-1 >= 0 		&& b.Get(currPos-1) && (currPos-1)!=prevPos) {			// Left
				return start;
		}
		else {			// if we cannot find any new points set back again.
			start = backTrack(ref b, ref path, start-1);
		}

		return start>0?start:-1;
	}

	// Get the initial adjancent point
	private bool nextPos(ref BinaryImage b, ref int pos, int prevPos) {
		int w = b.x;

		if(			prevPos+w < b.Length 	&& b.Get(prevPos+w)) pos = prevPos+w;		// Top
		else if((prevPos%w)+1 < w 		&& b.Get(prevPos+1)) pos = prevPos+1;		// Right
		else if(prevPos-w >= 0 				&& b.Get(prevPos-w)) pos = prevPos-w;		// Bottom
		else if((prevPos%w)-1 >= 0 		&& b.Get(prevPos-1)) pos = prevPos-1;		// Left
		else return false; // single pixel island

		return true;
	}

	// Get the adjancent point
	private bool nextPos(ref BinaryImage b, ref int pos, ref int currPos, ref int prevPos) {
		int w = b.x;

		if(			currPos+w < b.Length 	&& b.Get(currPos+w) && (currPos+w)!=prevPos) pos = currPos+w;		// Top
		else if((currPos%w)+1 < w 		&& b.Get(currPos+1) && (currPos+1)!=prevPos) pos = currPos+1;		// Right
		else if(currPos-w >= 0 	 			&& b.Get(currPos-w) && (currPos-w)!=prevPos) pos = currPos-w;		// Bottom
		else if((currPos%w)-1 >= 0 		&& b.Get(currPos-1) && (currPos-1)!=prevPos) pos = currPos-1;		// Left
		else return false;																																						// None

		// Update values
		prevPos = currPos;
		currPos = pos;

		return true;
	}

	// first stage of similification
	// Determine if we need to add this point(vertex) to the path
	private List<Vector2> simplify(ref BinaryImage b, List<Vector2> path) {
		List<Vector2> t = new List<Vector2>();

		t.Add(path[0]);

		// remove straight line vertices
		for(int i=1; i<path.Count-1; i++) {
			if(path[i-1].x != path[i+1].x && path[i-1].y != path[i+1].y) {
				t.Add(path[i]);
			}
		}

		// TODO: clean this up
		// give the points a weight A(k)=ab/2, a = [k-1]-k, b = [k+1]-k
		float[] weight = new float[t.Count];
		for(int k=1; k<t.Count-1; k++) {
			Vector2 i = t[k-1]-t[k], j = t[k+1]-t[k];

			weight[k] = ( (Mathf.Sqrt(i.x*i.x+i.y*i.y))*(Mathf.Sqrt(j.x*j.x+j.y*j.y))/2 );
		}

		List<Vector2> p = new List<Vector2>();
		p.Add(t[0]);
		for(int i=1; i<t.Count-1; i++) {
			if( weight[i] > 1) p.Add(t[i]); // TODO: find better constant / average weight? W = (w0+..+wk) / k
		}
		p.Add(t[t.Count-1]);

		return p;
	}

	/**
	 * Subtracts one binaryImage from another
	 * @param  {BinaryImage}  b1            [description]
	 * @param  {BinaryImage}  b2            [description]
	 * @return {BinaryImage}             [description]
	 */
	private BinaryImage subtraction(BinaryImage b1, BinaryImage b2) {
		BinaryImage t = new BinaryImage(b1.x, b1.y);

		int w = b1.x; // width
		int h = b1.y; // height

		for(int x=0; x<w; x++){
			for(int y=0; y<h; y++){
				t.Set(x+y*w, (b1.Get(x+y*w)!=b2.Get(x+y*w)) );
			}
		}
		return t;
	}

	/**
	 * If there is any true bits in a 3x3 grid make the centre bit false
	 * @param  {BinaryImage} ref BinaryImage   b [description]
	 * @return {BinaryImage}     The erosion image
	 */
	private BinaryImage erosion(BinaryImage b) {
		int[,] dirs = {{0,1},{1,1},{1,0},{1,-1},{0,-1},{-1,-1},{-1,0},{-1,1}};
		BinaryImage t = new BinaryImage(b.x, b.y, true);

		int w = b.x; // width
		int h = b.y;	// height

		for(int x=0; x<w; x++) {
			for(int y=0; y<h; y++) {
				// t.Set(x + y*w, true);
				for(int z=0; z<dirs.GetLength(0); z++) {
					int i = x+dirs[z,0], j = y+dirs[z,1];
					if(i<w && i>=0 && j<h && j>=0) {
						if(!b.Get(i + j*w)) t.Set(x + y*w, false);
					}
					else t.Set(x + y*w, false);
				}
			}
		}
		return t;
	}

	/**
	 * If the centre of a 3x3 is true then make the whole grid true
	 * @param  {BinaryImage} ref BinaryImage   b [description]
	 * @return {BinaryImage}     The dilated image
	 */
	private BinaryImage dilation(BinaryImage b) {
		int[,] dirs = {{0,1},{1,1},{1,0},{1,-1},{0,-1},{-1,-1},{-1,0},{-1,1}};
		BinaryImage t = new BinaryImage(b.x, b.y);

		int w = b.x; // width
		int h = b.y;	// height

		for(int x=0; x<w; x++){
			for(int y=0; y<h; y++){
				if(b.Get(x + y*w)) {
					for(int z=0; z<dirs.GetLength(0); z++) {
						int i = x+dirs[z,0], j = y+dirs[z,1];
						if(i<w && i>=0 && j<h && j>=0)
							t.Set(i + j*w, true);
					}
				}
			}
		}
		return t;
	}

	/**
	 * Generates a BitArray from a Texture2D
	 * @param  {Texture2D} t []
	 * @return {BitArray} [description]
	 */
	private BinaryImage BinaryImageFromTex(ref Texture2D t) {
		BinaryImage b = new BinaryImage(t.width, t.height);

		Color[] data = t.GetPixels();

		for(int x=0; x<b.Length; x++) b.Set(x, data[x].a > 0 );

		return b;
	}

	private Texture2D ApplyBinaryImage2Texture(ref Texture2D tex, ref BinaryImage b) {
		Texture2D t = new Texture2D(b.x, b.y);
		t.wrapMode = TextureWrapMode.Clamp;

		Color[] data = tex.GetPixels();

		for(int x=0; x<b.Length; x++){
			if(!b.Get(x)) data[x].a = 0;
		}

		t.SetPixels(data);
		t.Apply();

		return t;
	}

	/**
	 * Helper function to generate a Texture2D from a BinaryImage
	 * @param  {BinaryImage} b The BinaryImage to be converted to a Texture2D
	 * @return {Texture2D}     The generated Texture2D
	 */
	private Texture2D BinaryImage2Texture(BinaryImage b) {
		Texture2D t = new Texture2D(b.x, b.y);
		t.wrapMode = TextureWrapMode.Clamp;

		for(int x=0; x<t.width; x++) {
			for(int y=0; y<t.height; y++) {
				t.SetPixel(x, y, b.Get(x + y*b.x) ? Color.white : Color.black); // if true then white else black
			}
		}
		t.Apply();
		return t;
	}

	private Texture2D BinaryImage2TextureUsingPaths(BinaryImage b, List<List<Vector2> > paths) {
		List<Color> colorList = new List<Color>() {
			Color.red,
			Color.green,
			Color.blue,
			Color.magenta,
			Color.yellow
		};

		Texture2D t = new Texture2D(b.x, b.y);
		t.wrapMode = TextureWrapMode.Clamp;

		for(int i=0; i<paths.Count && i<colorList.Count; i++) {
			for(int j=0; j<paths[i].Count; j++) {
				t.SetPixel((int)paths[i][j].x, (int)paths[i][j].y, colorList[i]);
			}
		}
		t.Apply();
		return t;
	}

	private BinaryImage tidyBinaryImage(BinaryImage b) {
		return dilation(erosion(b));
	}
}
