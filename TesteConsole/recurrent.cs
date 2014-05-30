using contas.Helpers;
using Contas.Framework.BusinessLogicLayer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Linq;

namespace contas.Controllers.Apis
{
    public class RecurrentMovementsController : ApiController
    {
        public IEnumerable<RecurrentMovement> Get()
        {
            return RecurrentMovement.FindAllWithProperties();
        }

        public IEnumerable<RecurrentMovement> GetByUser(int idUser)
        {
            return RecurrentMovement.FindByIdUser(idUser);
        }

        public IEnumerable<RecurrentMovement> GetByTopRecurrentMovement(int idTopRecurrentMovement)
        {
            return RecurrentMovement.FindByTopRecurrentMovement(idTopRecurrentMovement).OrderBy(o=> o.SequenceNumber);
        }

        public RecurrentMovement Get(int id)
        {
            var recurrentMovement = RecurrentMovement.TryFind(id);
            if (recurrentMovement == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            return recurrentMovement;
        }

        public HttpResponseMessage Post([FromBody]RecurrentMovement recurrentMovement)
        {
            if (recurrentMovement.DthEnd == null)
                recurrentMovement.DthEnd = DateTime.MinValue;
            if (this.ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(recurrentMovement.PersonType))
                    {
                        var splited = recurrentMovement.PersonType.Split('-');
                        if (splited[0] == "c")
                            recurrentMovement.IdCustomer = int.Parse(splited[1]);
                        else if (splited[0] == "s")
                            recurrentMovement.IdSupplier = int.Parse(splited[1]);
                        else if (splited[0] == "l")
                            recurrentMovement.IdCollaborator = int.Parse(splited[1]);
                    }
                    //if (recurrentMovement.TopRecurrentMovement == 0)
                    //{
                    //    FinancialMovement.DeleteAll("DTH_REALIZATION IS NULL AND ID_RECURRENT_MOVEMENT =" + recurrentMovement.TopRecurrentMovement);
                    //    RecurrentMovement.DeleteAll("TOP_RECURRENT_MOVEMENT =" + recurrentMovement.TopRecurrentMovement);
                    //}
                    recurrentMovement.Save();
                    SaveFinancialMovement(recurrentMovement);
                    recurrentMovement = RecurrentMovement.FindWithProperties(recurrentMovement.Id);
                    var response = Request.CreateResponse<RecurrentMovement>(HttpStatusCode.Created, recurrentMovement);
                    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = recurrentMovement.Id }));
                    return response;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var errors = ModelState.GetAllErrors();
            return Request.CreateResponse(HttpStatusCode.BadRequest, errors);
        }

        public HttpResponseMessage Put(int id, [FromBody]RecurrentMovement recurrentMovement)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(recurrentMovement.PersonType))
                    {
                        var splited = recurrentMovement.PersonType.Split('-');
                        if (splited[0] == "c")
                            recurrentMovement.IdCustomer = int.Parse(splited[1]);
                        else if (splited[0] == "s")
                            recurrentMovement.IdSupplier = int.Parse(splited[1]);
                        else if (splited[0] == "l")
                            recurrentMovement.IdCollaborator = int.Parse(splited[1]);
                    }
                    if (recurrentMovement.TopRecurrentMovement == 0)
                    {
                        FinancialMovement.DeleteByTop(recurrentMovement.Id);
                        RecurrentMovement.DeleteAll("TOP_RECURRENT_MOVEMENT =" + recurrentMovement.Id);
                    }
                    else
                        recurrentMovement.Id = 0;
                    recurrentMovement.Save();

                    SaveFinancialMovement(recurrentMovement);
                    recurrentMovement = RecurrentMovement.FindWithProperties(recurrentMovement.Id);
                    var response = Request.CreateResponse<RecurrentMovement>(HttpStatusCode.OK, recurrentMovement);
                    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = recurrentMovement.Id }));
                    return response;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var errors = ModelState.GetAllErrors();
            return Request.CreateResponse(HttpStatusCode.BadRequest, errors);
        }

        public HttpResponseMessage Delete(int[] ids)
        {
            try
            {
                RecurrentMovement.Delete(ids);
                return Request.CreateResponse(HttpStatusCode.OK, ids);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public HttpResponseMessage Delete(int id)
        {
            var deleted = RecurrentMovement.TryFind(id);
            if (deleted == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            try
            {
                RecurrentMovement.DeleteAll("TOP_RECURRENT_MOVEMENT =" + id);
                RecurrentMovement.Delete(id);
                return Request.CreateResponse(HttpStatusCode.OK, deleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public HttpResponseMessage DeleteItens(int idTop)
        {
            try
            {
                FinancialMovement.DeleteByProperty("ID_RECURRENT_MOVEMENT", idTop);
                RecurrentMovement.DeleteAll("TOP_RECURRENT_MOVEMENT =" + idTop);
                return Request.CreateResponse(HttpStatusCode.OK, idTop);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #region PrivateMethods

        private static void SaveFinancialMovement(RecurrentMovement item) 
        {
            FinancialMovement financialMovement = new FinancialMovement();
            financialMovement.IdCompany = item.IdCompany;
            //financialMovement.IdOperationalUnit = item.IdOperationalUnit;
            if (!string.IsNullOrEmpty(item.PersonType))
            {
                //financialMovement.TopFinancialMovement = item.TopRecurrentMovement;
                var splited = item.PersonType.Split('-');
                if (splited[0] == "c")
                {
                    financialMovement.IdCustomer = int.Parse(splited[1]);
                    financialMovement.IdContabilAccountInverse = Customer.Find(financialMovement.IdCustomer).IdContabilAccount;
                }
                else if (splited[0] == "s")
                {
                    financialMovement.IdSupplier = int.Parse(splited[1]);
                    financialMovement.IdContabilAccountInverse = Supplier.Find(financialMovement.IdSupplier).IdContabilAccount;
                }
                else if (splited[0] == "l")
                {
                    financialMovement.IdCollaborator = int.Parse(splited[1]);
                    financialMovement.IdContabilAccountInverse = Collaborator.Find(financialMovement.IdCollaborator).IdContabilAccount;
                }
            }
            financialMovement.IdPaymentWay = item.IdPaymentWay;
            financialMovement.IdAccountPlan = item.IdAccountPlan;
            financialMovement.IdCostCenter = item.IdCostCenter;
            financialMovement.IdFinIndicatorMonetary = item.IdFinIndicatorMonetary;
            financialMovement.IdRecurrentMovement = item.Id;
            financialMovement.SequenceNumber = item.SequenceNumber;
            financialMovement.Description = item.Description;
            financialMovement.Observation = item.Observation;
            financialMovement.DocumentIdentification = item.DocumentIdentification;
            financialMovement.VlPredicted = item.VlPredicted;
            financialMovement.FinePercentage = item.FinePercentage;
            financialMovement.InterestPercentage = item.InterestPercentage;
            financialMovement.TlInterestType = item.TlInterestType;
            financialMovement.TlInterestPeriod = item.TlInterestPeriod;
            financialMovement.HonoraryPercentage = item.HonoraryPercentage;
            financialMovement.DiscountPercentage = item.DiscountPercentage;
            financialMovement.TlReleaseType = "02";
            financialMovement.TlReleaseSituation = "P";
            financialMovement.HasChild = item.HasChild;
            financialMovement.DthMaturity = DateTime.Parse(item.DthEnd.ToString());
            financialMovement.IdAccount = item.IdAccount;
            financialMovement.DthReference = (DateTime)item.DthStart;
            financialMovement.TlOriginMovement = TypeList.FindAllByProperty("abbreviation", "Movimento Recorrente")[0].Code;
            financialMovement.IdOperationalUnit = item.IdOperationalUnit;
            //if(item.TopRecurrentMovement > 0)
            //    financialMovement.TopFinancialMovement = FinancialMovement.Find(item.TopRecurrentMovement).Id;
            //financialMovement.VlRealized = item.IdDocumentMovement;
            //financialMovement.IdDocumentMovement = item.IdDocumentMovement;
            //financialMovement.IdContabilAccount = item.;
            //financialMovement.IdContabilAccountInverse = item.IdContabilAccountInverse;
            //financialMovement.IdFinancialMovementRelated = item.IdFinancialMovementRelated;
            //financialMovement.DthPredicted = item.DthPredicted; 
            //financialMovement.DthMaturity = item.DthMaturity;
            //financialMovement.DthCollection = item.DthCollection;
            //financialMovement.ParcelNumber = item.ParcelNumber;
            //financialMovement.ParcelsTotal = item.ParcelsTotal;
            //financialMovement.VlMonetaryIndicator = item.VlMonetaryIndicator;
            //financialMovement.DthRealization = item.DthRealization;
            //financialMovement.VlRealized = item.VlRealized;
            //financialMovement.DthRealization = item.DthRealization;
            //financialMovement.DthPrevious = item.DthPrevious;
            //financialMovement.VlPrevious = item.VlPrevious;
            //financialMovement.Reconciled = item.Reconciled;
            //financialMovement.UpdatedByUser = item.UpdatedByUser;
            //financialMovement.TlOriginMovement = item.TlOriginMovement;

            switch (item.TlMaturityPeriod)
            {
                case MaturityPeriods.Anual:
                    Anual(item, financialMovement);
                    break;
                case MaturityPeriods.Bimestral:
                    Bimestral(item, financialMovement);
                    break;
                case MaturityPeriods.Livre:
                    Livre(item, financialMovement);
                    break;
                case MaturityPeriods.Mensal:
                    Mensal(item, financialMovement);
                    break;
                case MaturityPeriods.Quadrimestral:
                    Quadrimensal(item, financialMovement);
                    break;
                case MaturityPeriods.Quinzenal:
                    Quinzenal(item, financialMovement);
                    break;
                case MaturityPeriods.Semanal:
                    Semanal(item, financialMovement);
                    break;
                case MaturityPeriods.Semestral:
                    Semestral(item, financialMovement);
                    break;
                case MaturityPeriods.Trimestral:
                    Trimestral(item, financialMovement);
                    break;
                default:
                    return;
                    break;
            }
        }

        private static void Anual(RecurrentMovement item, FinancialMovement financialMovement)
        {
            //var dataAux = new DateTime(item.DthStart.Year, item.DthStart.Month, int.Parse(item.MaturityMonthDay));
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddYears(1);
            }
        }

        private static void Mensal(RecurrentMovement item, FinancialMovement financialMovement)
        {

            if (item.TopRecurrentMovement > 0)
            {
                var date = (DateTime)RecurrentMovement.Find(item.TopRecurrentMovement).DthEnd;
                var dataAux = (DateTime)RecurrentMovement.Find(item.TopRecurrentMovement).DthStart;
                while (dataAux <= date)
                {
                    var father = FinancialMovement.FindAllByRecurrentMovement(item.TopRecurrentMovement).Where(w => w.DthPredicted.Month == dataAux.Month &&
                                                                                                                    w.DthPredicted.Year == dataAux.Year);
                    if (dataAux >= item.DthStart && father.Count() > 0)
                    {
                        financialMovement.TopFinancialMovement = father.First().Id;
                        financialMovement.DthPredicted = AddFeriado(dataAux, item);
                        financialMovement.Id = 0;
                        financialMovement.Save();
                    }
                    dataAux = dataAux.AddMonths(1);
                }
            }
            else
            {
                var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
                while (dataAux <= item.DthEnd)
                {
                    if (dataAux >= item.DthStart)
                    {
                        financialMovement.DthPredicted = AddFeriado(dataAux, item);
                        financialMovement.IdRecurrentMovement = item.Id;
                        financialMovement.Id = 0;
                        financialMovement.Save();
                    }
                    dataAux = dataAux.AddMonths(1);
                    //if (item.TlIntervalCalculationType == "02") // Dia util
                    //{
                    //    while (IsBusinessDay(item))
                    //        dataAux = dataAux.AddDays(1);
                    //}
                }
            }
        }

        private static void Bimestral(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddMonths(2);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }

        private static void Quadrimensal(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddMonths(4);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }

        private static void Semestral(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddMonths(6);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }

        private static void Trimestral(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddMonths(3);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }
        
        private static void Quinzenal(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while ((int)dataAux.DayOfWeek != int.Parse(item.TlMaturityWeekDay) - 1)
                dataAux = dataAux.AddDays(1);
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddDays(15);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }
        
        private static void Semanal(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, Convert.ToDateTime(item.DthStart).Day);
            while ((int)dataAux.DayOfWeek != int.Parse(item.TlMaturityWeekDay) - 1)
                dataAux = dataAux.AddDays(1);
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddDays(7);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }

        private static void Livre(RecurrentMovement item, FinancialMovement financialMovement)
        {
            var dataAux = new DateTime(Convert.ToDateTime(item.DthStart).Year, Convert.ToDateTime(item.DthStart).Month, int.Parse(item.MaturityMonthDay));
            while ((int)dataAux.DayOfWeek != int.Parse(item.TlMaturityWeekDay) - 1)
                dataAux = dataAux.AddDays(1);
            while (dataAux <= item.DthEnd)
            {
                if (dataAux >= item.DthStart)
                {
                    financialMovement.DthPredicted = AddFeriado(dataAux, item);
                    financialMovement.Id = 0;
                    financialMovement.Save();
                }
                dataAux = dataAux.AddDays((int)item.DaysInterval);
                //if (item.TlIntervalCalculationType == "02") // Dia util
                //{
                //    while (IsBusinessDay(item))
                //        dataAux = dataAux.AddDays(1);
                //}
            }
        }

        private static DateTime AddFeriado(DateTime data, RecurrentMovement recurrentMovement) 
        {
           int days = 0;
           if(recurrentMovement.TlActionNonworkingDay == Constants.ActionNonworkingDay.Adiantar)
            days = -1 ;
           else if (recurrentMovement.TlActionNonworkingDay == Constants.ActionNonworkingDay.Adiar)
            days = 1;
           var holidays = Holiday.FindAllByProperty("id_company", recurrentMovement.IdCompany);
           foreach (var item in holidays)
           if (days > 0)
            while (item.DthOccurrence == data)
                data = data.AddDays(days);
           return data;
        }

        private static bool IsBusinessDay(RecurrentMovement recurrentMovement)
        {
            DateTime data = (DateTime)recurrentMovement.DthEnd;
            if (data.DayOfWeek.ToString() == "Sunday" || data.DayOfWeek.ToString() == "Saturday")
                return true;
            var holidays = Holiday.FindAllByProperty("id_company", recurrentMovement.IdCompany);
            foreach (var item in holidays)
                if (data == item.DthOccurrence)
                    return true;
            return false;
        }
        #endregion
    }
}
