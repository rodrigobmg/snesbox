
namespace Snes
{
    partial class DSP1
    {
        private class SharedData
        { // some RAM variables shared between commands
            short[,] MatrixA = new short[3, 3]; // attitude matrix A
            short[,] MatrixB = new short[3, 3];
            short[,] MatrixC = new short[3, 3];
            short CentreX, CentreY, CentreZ; // center of projection
            short CentreZ_C, CentreZ_E;
            short VOffset; // vertical offset of the screen with regard to the centre of projection
            short Les, C_Les, E_Les;
            short SinAas, CosAas;
            short SinAzs, CosAzs;
            short SinAZS, CosAZS;
            short SecAZS_C1, SecAZS_E1;
            short SecAZS_C2, SecAZS_E2;
            short Nx, Ny, Nz; // normal vector to the screen (norm 1, points toward the center of projection)
            short Gx, Gy, Gz; // center of the screen (global coordinates)
            short Hx, Hy; // horizontal vector of the screen (Hz=0, norm 1, points toward the right of the screen)
            short Vx, Vy, Vz; // vertical vector of the screen (norm 1, points toward the top of the screen)
        }
    }
}
