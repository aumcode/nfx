"use strict";
/*jshint devel: true,browser: true, sub: true */ 
/*global WAVE: true,Stripe: true,$: true */
WAVE.Pay.Providers.Stripe = (function () { 

    var provider = { TYPE: "stripe" };

    // HTTP response status
    provider.HTTPStatus =
    {
        STATUS200: "OK	Everything worked as expected",
        STATUS400: "Bad Request	The request was unacceptable, often due to missing a required parameter",
        STATUS401: "Unauthorized	No valid API key provided",
        STATUS402: "Request Failed	The parameters were valid but the request failed",
        STATUS404: "Not Found	The requested resource doesn't exist",
        STATUS429: "Too Many Requests	Too many requests hit the API too quickly",
        STATUS50X: "500, 502, 503, 504 - Server Errors	Something went wrong on Stripe's end. (These are rare.)"      
    };

    // Maps Stripe HTTP response status to WAVE.Pay.HTTPStatus
    // https://stripe.com/docs/api#errors
    function mapStatus(status) {
        switch (status) {
            case "200": return provider.HTTPStatus.STATUS200; 
            case "400": return provider.HTTPStatus.STATUS400; 
            case "401": return provider.HTTPStatus.STATUS401; 
            case "402": return provider.HTTPStatus.STATUS402; 
            case "404": return provider.HTTPStatus.STATUS404; 
            case "429": return provider.HTTPStatus.STATUS429; 
            case "500": return provider.HTTPStatus.STATUS50X;     
            case "502": return provider.HTTPStatus.STATUS50X;     
            case "503": return provider.HTTPStatus.STATUS50X;     
            case "504": return provider.HTTPStatus.STATUS50X;     
        }
        return status;
    }
     
    // Maps Stripe response error type status to WAVE.Pay.ErrorTypes
    // https://stripe.com/docs/api#errors
    function mapErrorType(type) {
        switch (type) {
            case "api_connection_error": return WAVE.Pay.ErrorTypes.API_CONNECTION_ERROR;
            case "api_error": return WAVE.Pay.ErrorTypes.API_ERROR;                    
            case "authentication_error": return WAVE.Pay.ErrorTypes.AUTHENTICATION_ERROR; 
            case "card_error": return WAVE.Pay.ErrorTypes.CARD_ERROR;                   
            case "invalid_request_error": return WAVE.Pay.ErrorTypes.INVALID_REQUEST_ERROR;  
            case "rate_limit_error": return WAVE.Pay.ErrorTypes.RATE_LIMIT_ERROR;       
        }
        return WAVE.Pay.ErrorTypes.OK;
    }
    
    // Maps Stripe response card error codes status to WAVE.Pay.CardErrors
    // https://stripe.com/docs/api#errors
    function mapErrorCode(code) {
        switch (code) {
            case "invalid_number": return WAVE.Pay.CardErrors.INVALID_NUMBER;
            case "invalid_expiry_month": return WAVE.Pay.CardErrors.INVALID_EXPIRY_MONTH; 
            case "invalid_expiry_year": return WAVE.Pay.CardErrors.INVALID_EXPIRY_YEAR;
            case "invalid_cvc": return WAVE.Pay.CardErrors.INVALID_CVC;           
            case "incorrect_number": return WAVE.Pay.CardErrors.INCORRECT_NUMBER;    
            case "expired_card": return WAVE.Pay.CardErrors.EXPIRED_CARD;           
            case "incorrect_cvc": return WAVE.Pay.CardErrors.INCORRECT_CVC;        
            case "incorrect_zip": return WAVE.Pay.CardErrors.INCORRECT_ZIP;        
            case "card_declined": return WAVE.Pay.CardErrors.CARD_DECLINED;        
            case "missing": return WAVE.Pay.CardErrors.MISSING;                   
            case "processing_error": return WAVE.Pay.CardErrors.PROCESSING_ERROR;   
        }
        return WAVE.Pay.CardErrors.OK;
    }
  
    // Gets Stripe card info 
    // https://stripe.com/docs/api#token_object
    function mapCard(card) {
      var result = {
              "providerID": card.id,
              "addressCity": card.address_city,
              "addressCountry": card.address_country,
              "addressLine1": card.address_line1,
              "addressLine2": card.address_line2,
              "addressState": card.address_state,
              "addressZip": card.address_zip,
              "brand": card.brand,
              "CVCcheck": card.cvc_check,
              "expirationMonth": card.exp_month,
              "expirationYear": card.exp_year,
              "fundingType": card.funding,
              "last4": card.last4,
              "holder": card.name
            };
        return result;
    }

    // Payment provider setup (like setting public keys)
    provider.initialize = function (init) {
        if (WAVE.strEmpty(init.publicKey))
            throw new WAVE.Pay.RequiredArgumentError("WAVE.Pay.Providers.Stripe.initialize()", "init.publicKey");

        Stripe.setPublishableKey(init.publicKey);
    }; 

    // Payment provider tokenization interface
    provider.tokenize = function (facade, paymentData, callback) {
        if (WAVE.strEmpty(paymentData.cardNumber)||
            WAVE.strEmpty(paymentData.cardExpMonth) ||
            WAVE.strEmpty(paymentData.cardExpYear))
            throw new WAVE.Pay.RequiredArgumentError("WAVE.Pay.Providers.Stripe.tokenize()", "paymentData.cardNumber, paymentData.cardExpMonth, paymentData.cardExpYear");

        // prepare stripe.js init card data object
        var stripeCardData = 
            { 
                number: paymentData.cardNumber.replace(/\s+|-/g, ""), 
                exp_month: paymentData.cardExpMonth.length === 2 ? "0" + paymentData.cardExpMonth : paymentData.cardExpMonth,
                exp_year: paymentData.cardExpYear
            };
        if (!WAVE.strEmpty(paymentData.cardCVC))
            stripeCardData.cvc = paymentData.cardCVC;
        if (!WAVE.strEmpty(paymentData.cardHolder))
            stripeCardData.name = paymentData.cardHolder;
        if (!WAVE.strEmpty(paymentData.cardAddressLine1))
            stripeCardData.address_line1 = paymentData.cardAddressLine1;
        if (!WAVE.strEmpty(paymentData.cardAddressLine2))
            stripeCardData.address_line2 = paymentData.cardAddressLine2;
        if (!WAVE.strEmpty(paymentData.cardAddressCity))
            stripeCardData.address_city = paymentData.cardAddressCity;
        if (!WAVE.strEmpty(paymentData.cardAddressState))
            stripeCardData.address_state = paymentData.cardAddressState;
        if (!WAVE.strEmpty(paymentData.cardAddressZip))
            stripeCardData.address_zip = paymentData.cardAddressZip; 
        if (!WAVE.strEmpty(paymentData.cardAddressCountry))
            stripeCardData.address_country = paymentData.cardAddressCountry;

        // prepare stripe.js response callback which internally calls outer 'callback' function
        var responseHandler = function(status, response) { 
            var result = 
                { 
                  requestContext: { "facade": facade, "paymentData": paymentData },
                    OK: status >= 200 && status < 300,
                    httpStatus: status,
                    httpStatusDescr: mapStatus(status),
                    providerType: provider.TYPE,
                    providerName: facade.providerName(),
                    errorCode: 0,
                    errorType: 0,
                    errorMessage: "",
                    errorParams: null,
                    rawError: null,
                    tokenString: null,
                    card: { }
                };

            if (!WAVE.strEmpty(response.error)) {
                result.OK = false;
                result.errorCode = mapErrorCode(response.error.code);
                result.errorType = mapErrorType(response.error.type);
                result.errorMessage = response.error.message;
                result.errorParams = { "0": response.error.param };
                result.rawError = response.error;
            }
            else {
                result.tokenString = response.id;
                result.card = mapCard(response.card);
            }

            callback(result);
        };

        // call stripe.js to obtain card payment token
        Stripe.card.createToken(stripeCardData, responseHandler);   
    };

    return provider;

}());