using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexUtils {

	[System.Serializable]
	public struct HexVec : IEquatable<HexVec> {

		const float HALFSQRT3 = 0.866025403f;
		const float SQRT3 = 1.73205080757f;
		public int x;
		public int y;
		public readonly int Z {
			get { return (-x - y); }
		}

		public static HexVec Zero = new HexVec { x = 0, y = 0 };
		public static HexVec Max = new HexVec { x = int.MaxValue, y = int.MaxValue };
		public static HexVec[] directions = new HexVec[6] { new HexVec(1, 0), new HexVec(0, 1), new HexVec(-1, 1), new HexVec(-1, 0), new HexVec(0, -1), new HexVec(1, -1) };

		//Slide y axis
		public static HexVec Right = directions[0];
		public static HexVec Left = directions[3];

		//Slide x axis
		public static HexVec UpRight = directions[1];
		public static HexVec DownLeft = directions[4];

		//Slide z axis
		public static HexVec UpLeft = directions[2];
		public static HexVec DownRight = directions[5];


		//anticlockwise from bottomright
		public static UnityEngine.Vector3[] hexPoints = new UnityEngine.Vector3[] {
			new UnityEngine.Vector3(HALFSQRT3, 0, -0.5f),
			new UnityEngine.Vector3(HALFSQRT3, 0, 0.5f),
			new UnityEngine.Vector3(0, 0, 1),
			new UnityEngine.Vector3(-HALFSQRT3, 0, 0.5f),
			new UnityEngine.Vector3(-HALFSQRT3, 0, -0.5f),
			new UnityEngine.Vector3(0, 0, -1)
		};

		public enum DirectionNumber {
			Right = 0,
			UpRight = 1,
			UpLeft = 2,
			Left = 3,
			DownLeft = 4,
			DownRight = 5
		}


		public HexVec(int x, int y) { this.x = x; this.y = y; }
		public static HexVec operator +(HexVec a, HexVec b) { return new HexVec { x = a.x + b.x, y = a.y + b.y }; }
		public static HexVec operator *(HexVec a, int b) { return new HexVec { x = a.x * b, y = a.y * b }; }

		public static HexVec[] Circle (int radius) { return Circle(radius, Zero);}
		public static HexVec[] Circle(int radius, HexVec offset) {
			HexVec[] ret = new HexVec[(radius - 1) * radius * 3 + 1];
			int i = 0;
			for (int x = 1; x < radius; x++) {
				for (int y = 0; y < x; y++) {
					for (int rotations = 0; rotations < 6; rotations++) {
						ret[i] = (new HexVec(x - y, y)).Rotate(rotations) + offset;
						i++;
					}
				}
			}
			return ret;
		}

		public bool Equals(HexVec obj) {
			return (obj.x == x && obj.y == y);
		}

		public override int GetHashCode() {
			return x + (y << 16);
		}

		public static bool operator == (HexVec a, HexVec b){
			return (a.x == b.x && a.y == b.y);
		}

		public static bool operator !=(HexVec a, HexVec b) {
			return !(a==b);
		}

		public override string ToString() {
			return $"{x} {y} ({Z})";
		}
		public HexVec Rotate(bool antiClockwise = false) {
			return antiClockwise ? Rotate(-1) : Rotate(1);
		}

		public UnityEngine.Vector3 ToVector3(float height = 0f) {
			return new UnityEngine.Vector3(HALFSQRT3 * x + HALFSQRT3 * -Z , height, 1.5f * y);
		}

		public HexVec Rotate(int rotations) {
			if (rotations == 0) return this;
			HexVec ret = this;
			if(rotations < 0) {
				for(int i = 0; i > rotations; i--) {
					int newX = ret.y * -1;
					ret.y = ret.Z * -1;
					ret.x = newX;
				}
			} else {
				for (int i = 0; i < rotations; i++) {
					int newX = ret.Z * -1;
					ret.y = ret.x * -1;
					ret.x = newX;
				}
			}
			return ret;
		}



			
	}
}
