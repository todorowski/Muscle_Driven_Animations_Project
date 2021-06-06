using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AUTHOR: Phylliida
//FROM https://github.com/Phylliida/openmuscle/blob/master/OpenMuscle.cs

public class Muscle2
{
    ContractileElement CE;
    ParallelElasticElement PEE;
    SerialDampingElement SDE;
    SerialElasticElement SEE;

    //Length and velocity of musculotendon complex
    double l_MTC;
    double v_MTC;

    //Length and velocity of the contractile element
    double l_CE;
    double v_CE;


    public Muscle2(double q)
    {
        CE = new ContractileElement();
        PEE = new ParallelElasticElement(CE);
        SDE = new SerialDampingElement(CE);
        SEE = new SerialElasticElement();
        
        //This length may vary for different muscles
        double l_MTC_init = 0.92 + 0.172;  // [m] initial MTC length
        double q_CE_init = q;          // [] initial muscle activity 0...1

        //The initial muslce length is based on the activation value q
        double l_CE_init = getl_CE_init(q_CE_init);
        
        l_MTC = l_MTC_init;
        l_CE = l_CE_init;

        v_MTC = 0.0;
        v_CE = 0.0;
    }

    public double step(float q, float dt)
    {
        //The velocity of the CE is obtained by solving differential equation in compute_v_ce
        double v_CE = compute_v_ce(l_CE, l_MTC, q);

        double f_see = this.f_see(l_CE, l_MTC);
        double f_sde = this.f_sde(l_CE, q);
        double f_mtc = f_see + f_sde;

        double a_mtc = f_mtc / 1000;

        this.v_CE = v_CE;

        l_CE += this.v_CE * dt;

        return (float)a_mtc;
    }

    //The serial damping element is arranged in parallel to the serial elastic element
    double f_sde(double l_CE, double q)
    {
        return SDE.d_SEmax * ((1 - SDE.R_SDE) * (f_ce(l_CE, q) + f_pee(l_CE)) / CE.F_max + SDE.R_SDE) * (v_MTC - v_CE);
    }

    //The isometric force models constraction against resistance in which the length of the muscle stays the same
    double f_isom(double l_CE)
    {
        // Isometric force (Force length relation)
        // Guenther et al. 2007
        double F_isom;
        if (l_CE >= CE.l_CEopt) // descending branch:
            F_isom = Mathf.Exp(-Mathf.Pow((Mathf.Abs((float)((l_CE / CE.l_CEopt) - 1) / (float)CE.DeltaW_limb_des)), (float)CE.v_CElimb_des));
        else // ascending branch
            F_isom = Mathf.Exp(-Mathf.Pow((Mathf.Abs((float)((l_CE / CE.l_CEopt) - 1) / (float)CE.DeltaW_limb_asc)), (float)CE.v_CElimb_asc));
        return F_isom;
    }

    //The parallel elastic element is arranged in parallel to the CE
    double f_pee(double l_CE)
    {
        // Force of the parallel elastic element
        double F_PEE;

        if (l_CE >= PEE.l_PEE0)
            F_PEE = PEE.K_PEE * Mathf.Pow((float)(l_CE - PEE.l_PEE0), (float)(PEE.v_PEE));
        else // shorter than slack length
            F_PEE = 0;
        return F_PEE;
    }

    double f_see(double l_CE, double l_MTC)
    {
        // Force of the serial elastic element

        double l_SEE = l_MTC - l_CE;
        double F_SEE;
        if (l_SEE > SEE.l_SEE0 && l_SEE < SEE.l_SEEnll) // non-linear part
            F_SEE = SEE.KSEEnl * (Mathf.Pow((float)(l_SEE - SEE.l_SEE0), (float)(SEE.v_SEE)));
        else if (l_SEE >= SEE.l_SEEnll) // linear part
            F_SEE = SEE.DeltaF_SEE0 + SEE.KSEEl * (l_SEE - SEE.l_SEEnll);
        else // slack length
            F_SEE = 0;

        return F_SEE;
    }

    //The force of the contractile element
    //depends on current fiber contraction velocity
    //And the force-velocity relation
    //l_CEopt is the optimal fiber length for which the isometric force reaches a maximum
    double f_ce(double l_CE, double q)
    {
        //Normalized Hill Parameters
        double aRel, bRel;
        getabRel(l_CE, q, false, out aRel, out bRel);

        return CE.F_max * (
          (q * f_isom(l_CE) + aRel) /
          (1 - v_CE / (bRel * CE.l_CEopt))
          - aRel
          );
    }

    double l_Arel(double l_CE)
    {
        if (l_CE < CE.l_CEopt) return 1.0;
        else return f_isom(l_CE);
    }

    double l_Brel()
    {
        return 1.0;
    }

    double q_Arel(double q)
    {
        return 1.0 / 4.0 * (1.0 + 3.0 * q);
    }

    double q_Brel(double q)
    {
        return 1.0 / 7.0 * (3.0 + 4.0 * q);
    }


    //Parameter s_e describes the ratio of the derivatives of the force-velocity relation at the transition point 
    void getabRel(double l_CE, double q, bool getC, out double aRel, out double bRel)
    {
        aRel = CE.A_rel0 * l_Arel(l_CE) * q_Arel(q);
        bRel = CE.B_rel0 * l_Brel() * q_Brel(q);

        if (getC) //this.v_CE > 0:
        {
            double f_isom = this.f_isom(l_CE);

            double f_e = CE.F_eccentric;
            double s_e = CE.S_eccentric;

            double aRelC = -f_e * q * f_isom;
            double bRelC = bRel * (1 - f_e) /
               (s_e * (1 + (aRel / (q * f_isom))));

            aRel = aRelC;
            bRel = bRelC;
        }
    }

    double compute_v_ce(double l_CE, double l_MTC, double q)
    {
        double f_pee = this.f_pee(l_CE);
        double f_isom = this.f_isom(l_CE);
        double f_see = this.f_see(l_CE, l_MTC);
        double r_se = SDE.R_SDE;
        double f_max = CE.F_max;
        double l_ceOpt = CE.l_CEopt;
        double d_seMax = SDE.d_SEmax;
        double v_mtc = v_MTC;
        double aRel, bRel;

        getabRel(l_CE, q, false, out aRel, out bRel);

        double d0 = l_ceOpt * bRel * d_seMax * (r_se + (1 - r_se) * (q * f_isom + f_pee / f_max));
        double c2 = d_seMax * (r_se - (aRel - f_pee / f_max) * (1 - r_se));
        double c1 = -(c2 * v_mtc + d0 + f_see - f_pee + f_max * aRel);
        double c0 = d0 * v_mtc + l_ceOpt * bRel * (f_see - f_pee - f_max * q * f_isom);

        double v_CE = (-c1 - Mathf.Sqrt((float)(c1 * c1 - 4 * c2 * c0))) / (2 * c2);

        if (v_CE <= 0)
        {
            getabRel(l_CE, q, true, out aRel, out bRel);

            d0 = l_ceOpt * bRel * d_seMax * (r_se + (1 - r_se) * (q * f_isom + f_pee / f_max));
            c2 = d_seMax * (r_se - (aRel - f_pee / f_max) * (1 - r_se));
            c1 = -(c2 * v_mtc + d0 + f_see - f_pee + f_max * aRel);
            c0 = d0 * v_mtc + l_ceOpt * bRel * (f_see - f_pee - f_max * q * f_isom);

            return (-c1 + Mathf.Sqrt((float)(c1 * c1 - 4 * c2 * c0))) / (2 * c2);
        }
        else
        {
            return v_CE;
        }
    }

    double getMuscleForceInit(double l_CE, double l_MTC, double q)
    {
        double F_SEE = f_see(l_CE, l_MTC);
        double F_CE = q * f_isom(l_CE) * CE.F_max;
        double F_PEE = f_pee(l_CE);

        double F_sum = F_SEE - F_CE - F_PEE;

        return F_sum;
    }

    private double getl_CE_init(double q)
    {
        q = Mathf.Min(Mathf.Max((float)q, 0.001f), 1.0f);
        
        q *= 100;
     
        q = Mathf.Round((float)q);

        int value = ((int)q - 1);
        if (value < 1)
            value = 1;
        if (q > 100)
            value = 100;

        return initialLengths[value];

    }

    static double[] initialLengths = new double[] {
      0.893969762284,
      0.891389588719,
      0.889046126956,
      0.886891942301,
      0.884892999683,
      0.883024013777,
      0.881265656012,
      0.879602796499,
      0.878023352693,
      0.876517509536,
      0.875077175724,
      0.873695594951,
      0.872367061816,
      0.871086710171,
      0.869850352706,
      0.868654357513,
      0.867495551782,
      0.866371145748,
      0.865278671931,
      0.86421593612,
      0.863180977442,
      0.86217203557,
      0.861187523561,
      0.86022600521,
      0.859286176034,
      0.858366847203,
      0.857466931886,
      0.856585433582,
      0.855721436094,
      0.854874094887,
      0.854042629585,
      0.853226317445,
      0.852424487656,
      0.851636516328,
      0.850861822091,
      0.850099862197,
      0.849350129076,
      0.848612147265,
      0.847885470674,
      0.847169680144,
      0.846462475631,
      0.845756343231,
      0.845050575307,
      0.844345193694,
      0.843640220436,
      0.842935677794,
      0.842231588239,
      0.841527974462,
      0.840824859375,
      0.840122266114,
      0.839420218046,
      0.838718738777,
      0.838017852163,
      0.837317582323,
      0.836617953654,
      0.835918990854,
      0.835220718949,
      0.834523163328,
      0.833826349788,
      0.833130304599,
      0.832435054583,
      0.831740627232,
      0.831047050878,
      0.830354354951,
      0.829662570414,
      0.828971730574,
      0.828281873078,
      0.827593048977,
      0.826905305325,
      0.826218658243,
      0.825533122074,
      0.824848711093,
      0.824165439497,
      0.823483321402,
      0.822802370841,
      0.822122601756,
      0.821444027999,
      0.820766663322,
      0.820090521378,
      0.819415615714,
      0.81874195977,
      0.818069566871,
      0.817398450227,
      0.816728622929,
      0.816060097943,
      0.815392888108,
      0.814727006134,
      0.814062464596,
      0.813399275932,
      0.812737452442,
      0.812077006279,
      0.811417949452,
      0.810760293822,
      0.810104051095,
      0.809449232824,
      0.808795850406,
      0.808143915074,
      0.807493437904,
      0.806844429803,
      0.806196901512
   };
}


class ContractileElement
{
    public double F_max = 1420.0;               // F_max in [N] for Extensor (Kistemaker et al., 2006)
    public double l_CEopt = 0.92;              // optimal length of CE in [m] for Extensor (Kistemaker et al., 2006)
    public double DeltaW_limb_des = 0.35;       // width of normalized bell curve in descending branch (Moerl et al., 2012)
    public double DeltaW_limb_asc = 0.35;       // width of normalized bell curve in ascending branch (Moerl et al., 2012)
    public double v_CElimb_des = 1.5;           // exponent for descending branch (Moerl et al., 2012)
    public double v_CElimb_asc = 3.0;           // exponent for ascending branch (Moerl et al., 2012)
    public double A_rel0 = 0.25;                // parameter for contraction dynamics: maximum value of A_rel (Guenther, 1997, S. 82)
    public double B_rel0 = 2.25;                // parameter for contraction dynmacis: maximum value of B_rel (Guenther, 1997, S. 82)

    // eccentric force-velocity relation:
    public double S_eccentric = 2.0;            // relation between F(v) slopes at v_CE=0 (van Soest & Bobbert, 1993)
    public double F_eccentric = 1.5;            // factor by which the force can exceed F_isom for large eccentric velocities (van Soest & Bobbert, 1993)
}

class ParallelElasticElement
{

    public ParallelElasticElement(ContractileElement ce)
    {
        l_PEE0 = L_PEE0 * ce.l_CEopt;
        K_PEE = F_PEE * (ce.F_max / Mathf.Pow((float)(ce.l_CEopt * (ce.DeltaW_limb_des + 1 - L_PEE0)), (float)v_PEE));
    }


    public double L_PEE0 = 0.9;                               // rest length of PEE normalized to optimal lenght of CE (Guenther et al., 2007)
    public double l_PEE0;                                     // rest length of PEE (Guenther et al., 2007)
    public double v_PEE = 2.5;                                // exponent of F_PEE (Moerl et al., 2012)
    public double F_PEE = 2.0;                                // force of PEE if l_CE is stretched to deltaWlimb_des (Moerl et al., 2012)
    public double K_PEE;                                      // factor of non-linearity in F_PEE (Guenther et al., 2007)
}

class SerialDampingElement
{
    public SerialDampingElement(ContractileElement ce)
    {
        d_SEmax = D_SDE * (ce.F_max * ce.A_rel0) / (ce.l_CEopt * ce.B_rel0);
    }

    public double d_SEmax;                   // maximum value in d_SE in [Ns/m] (Moerl et al., 2012)
    public double D_SDE = 0.3;               // xxx dimensionless factor to scale d_SEmax (Moerl et al., 2012)
    public double R_SDE = 0.01;              // minimum value of d_SE normalised to d_SEmax (Moerl et al., 2012)
}


class SerialElasticElement
{
    public SerialElasticElement()
    {
        l_SEEnll = (1 + DeltaU_SEEnll) * l_SEE0;
        v_SEE = DeltaU_SEEnll / DeltaU_SEEl;
        KSEEnl = DeltaF_SEE0 / Mathf.Pow((float)(DeltaU_SEEnll * l_SEE0), (float)v_SEE);
        KSEEl = DeltaF_SEE0 / (DeltaU_SEEl * l_SEE0);
    }

    public double l_SEE0 = 0.172;              // rest length of SEE in [m] (Kistemaker et al., 2006)
    public double DeltaU_SEEnll = 0.0425;      // relativ stretch at non-linear linear transition (Moerl et al., 2012)
    public double DeltaU_SEEl = 0.017;         // relativ additional stretch in the linear part providing a force increase of deltaF_SEE0 (Moerl, 2012)
    public double DeltaF_SEE0 = 568.0;         // both force at the transition and force increase in the linear part in [N] (~ 40% of the maximal isometric muscle force)

    public double l_SEEnll;
    public double v_SEE;
    public double KSEEnl;
    public double KSEEl;
}
