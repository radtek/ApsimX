using System;
using Models.Core;
using Models.Functions;
using Models.PMF.Interfaces;
using Models.PMF.Library;
using Models.Interfaces;
using System.Xml.Serialization;

namespace Models.PMF.Organs
{
    /// <summary>
    /// A harvest index reproductive organ
    /// </summary>
    [Serializable]
    public class HIReproductiveOrgan : Model, IOrgan, IArbitration, IRemovableBiomass
    {
        /// <summary>The surface organic matter model</summary>
        [Link]
        public ISurfaceOrganicMatter SurfaceOrganicMatter = null;

        /// <summary>The plant</summary>
        [Link]
        protected Plant Plant = null;

        /// <summary>Gets or sets the above ground.</summary>
        [Link]
        IFunction AboveGroundWt = null;

        /// <summary>The water content</summary>
        [Link]
        IFunction WaterContent = null;
        /// <summary>The hi increment</summary>
        [Link]
        IFunction HIIncrement = null;
        /// <summary>The n conc</summary>
        [Link]
        IFunction NConc = null;

        /// <summary>Link to biomass removal model</summary>
        [ChildLink]
        public BiomassRemoval biomassRemovalModel = null;

        /// <summary>The daily growth</summary>
        private double DailyGrowth = 0;

        /// <summary>The live biomass</summary>
        public Biomass Live { get; set; }

        /// <summary>The dead biomass</summary>
        public Biomass Dead { get; set; }

        /// <summary>Gets a value indicating whether the biomass is above ground or not</summary>
        public bool IsAboveGround { get { return true; } }

        /// <summary>Growth Respiration</summary>
        /// [Units("CO_2")]
        public double GrowthRespiration { get; set; }


        /// <summary>Gets the biomass allocated (represented actual growth)</summary>
        [XmlIgnore]
        public Biomass Allocated { get; set; }

        /// <summary>Gets the biomass senesced (transferred from live to dead material)</summary>
        [XmlIgnore]
        public Biomass Senesced { get; set; }

        /// <summary>Gets the DM amount detached (sent to soil/surface organic matter) (g/m2)</summary>
        [XmlIgnore]
        public Biomass Detached { get; set; }

        /// <summary>Gets the DM amount removed from the system (harvested, grazed, etc) (g/m2)</summary>
        [XmlIgnore]
        public Biomass Removed { get; set; }

        /// <summary>The amount of mass lost each day from maintenance respiration</summary>
        virtual public double MaintenanceRespiration { get { return 0; } set { } }

        /// <summary>The dry matter demand</summary>
        protected BiomassPoolType dryMatterDemand = new BiomassPoolType();

        /// <summary>Structural nitrogen demand</summary>
        protected BiomassPoolType nitrogenDemand = new BiomassPoolType();

        /// <summary>Calculate and return the nitrogen supply (g/m2)</summary>
        public  BiomassSupplyType GetNitrogenSupply()
        {
            return new BiomassSupplyType();
        }

        /// <summary>Sets the dm potential allocation.</summary>
        /// <summary>Sets the dry matter potential allocation.</summary>
         public void SetDryMatterPotentialAllocation(BiomassPoolType dryMatter) { }
        /// <summary>Gets or sets the dm demand.</summary>
        [XmlIgnore]
         public BiomassPoolType DMDemand { get { return dryMatterDemand; } }
        /// <summary>the efficiency with which allocated DM is converted to organ mass.</summary>

        /// <summary>Gets or sets the n fixation cost.</summary>
        [XmlIgnore]
         public double NFixationCost { get { return 0; } }
        /// <summary>Gets or sets the n demand.</summary>
        [XmlIgnore]
         public BiomassPoolType NDemand { get { return nitrogenDemand; } }
        /// <summary>Gets or sets the minimum nconc.</summary>
        [XmlIgnore]
         public double MinNconc { get { return 0; } }




        /// <summary>Gets the live f wt.</summary>
        /// <value>The live f wt.</value>
        [Units("g/m^2")]
        public double LiveFWt
        {
            get
            {

                if (WaterContent != null)
                    return Live.Wt / (1 - WaterContent.Value());
                else
                    return 0.0;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="HIReproductiveOrgan"/> class.</summary>
        public HIReproductiveOrgan()
        {
            Live = new Biomass();
            Dead = new Biomass();
        }

        /// <summary>Called when [do daily initialisation].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoDailyInitialisation")]
        protected void OnDoDailyInitialisation(object sender, EventArgs e)
        {
            if (Plant.IsAlive)
            {
                Allocated.Clear();
                Senesced.Clear();
                Detached.Clear();
                Removed.Clear();

            }
        }

        /// <summary>Called when [simulation commencing].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        protected void OnSimulationCommencing(object sender, EventArgs e)
        {
            Allocated = new PMF.Biomass();
            Senesced = new Biomass();
            Detached = new Biomass();
            Removed = new Biomass();
        }

        /// <summary>Called when crop is ending</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantSowing")]
        private void OnPlantSowing(object sender, SowPlant2Type data)
        {
                Clear();
        }

        /// <summary>Called when crop is ending</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantEnding")]
        private void OnPlantEnding(object sender, EventArgs e)
        {
            Biomass total = Live + Dead;
            if (total.Wt > 0.0)
            {
                Detached.Add(Live);
                Detached.Add(Dead);
                SurfaceOrganicMatter.Add(total.Wt * 10, total.N * 10, 0, Plant.CropType, Name);
            }

            Clear();
        }

        /// <summary>Gets the hi.</summary>
        /// <value>The hi.</value>
        public double HI
        {
            get
            {
                double CurrentWt = (Live.Wt + Dead.Wt);
                if (AboveGroundWt.Value() > 0)
                    return CurrentWt / AboveGroundWt.Value();
                else
                    return 0.0;
            }
        }

        /// <summary>Sets the dry matter allocation.</summary>
        public void SetDryMatterAllocation(BiomassAllocationType dryMatter)
        {
            Live.StructuralWt += dryMatter.Structural; DailyGrowth = dryMatter.Structural;
        }

        /// <summary>Sets the n allocation.</summary>
        public  void SetNitrogenAllocation(BiomassAllocationType nitrogen)
        {
            Live.StructuralN += nitrogen.Structural;
        }

        /// <summary>Gets the total biomass</summary>
        public Biomass Total { get { return Live + Dead; } }

        /// <summary>Gets the total grain weight</summary>
        [Units("g/m2")]
        public double Wt { get { return Total.Wt; } }

        /// <summary>Gets the total grain N</summary>
        [Units("g/m2")]
        public double N { get { return Total.N; } }

        /// <summary>Calculate and return the dry matter demand (g/m2)</summary>
        public BiomassPoolType GetDryMatterDemand()
        {
            double currentWt = (Live.Wt + Dead.Wt);
            double newHI = HI + HIIncrement.Value();
            double newWt = newHI * AboveGroundWt.Value();
            double demand = Math.Max(0.0, newWt - currentWt);
            dryMatterDemand.Structural = demand;
            return dryMatterDemand;
        }

        /// <summary>Calculate and return the nitrogen demand (g/m2)</summary>
        public BiomassPoolType GetNitrogenDemand()
        {
            double demand = Math.Max(0.0, (NConc.Value() * Live.Wt) - Live.N);
            nitrogenDemand.Structural = demand;
            return nitrogenDemand;
        }

        /// <summary>Remove maintenance respiration from live component of organs.</summary>
        /// <param name="respiration">The respiration to remove</param>
        public virtual void RemoveMaintenanceRespiration(double respiration)
        {
            double total = Live.MetabolicWt + Live.StorageWt;
            if (respiration > total)
            {
                throw new Exception("Respiration is more than total biomass of metabolic and storage in live component.");
            }
            Live.MetabolicWt = Live.MetabolicWt - (respiration * Live.MetabolicWt / total);
            Live.StorageWt = Live.StorageWt - (respiration * Live.StorageWt / total);
        }

        /// <summary>Removes biomass from organs when harvest, graze or cut events are called.</summary>
        /// <param name="biomassRemoveType">Name of event that triggered this biomass remove call.</param>
        /// <param name="value">The fractions of biomass to remove</param>
        public void RemoveBiomass(string biomassRemoveType, OrganBiomassRemovalType value)
        {
            biomassRemovalModel.RemoveBiomass(biomassRemoveType, value, Live, Dead, Removed, Detached);
        }

        /// <summary>Clears this instance.</summary>
        private void Clear()
        {
            Live.Clear();
            Dead.Clear();
            dryMatterDemand.Clear();
            nitrogenDemand.Clear();
        }

        /// <summary>Calculate and return the dry matter supply (g/m2)</summary>
        public BiomassSupplyType GetDryMatterSupply()
        {
            return new BiomassSupplyType();
        }
    }
}
