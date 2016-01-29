"use strict";
/*jshint devel: true,browser: true, sub: true */ 
/*global WAVE: true,braintree: true,$: true */
WAVE.Pay.Providers.Braintree = (function () { 

    function mapStatus(status) {
        return null;
    }

    function mapErrorType(type) {
        if (!WAVE.strEmpty(type))
            return WAVE.Pay.ErrorTypes.GENERAL_ERROR;
        return WAVE.Pay.ErrorTypes.OK;
    }
    
    function mapErrorCode(code) {
        if (!WAVE.strEmpty(code))
            return WAVE.Pay.CardErrors.GENERAL_ERROR;
        return WAVE.Pay.CardErrors.OK;
    }
  
    function mapCard(card) {
        return null;
    }

    var provider = 
        { 
            TYPE: "braintree", 
            clientToken: null 
        };

    // Payment provider setup (like setting public keys)
    provider.initialize = function (init) {
        if (WAVE.strEmpty(init.publicKey))
            throw new WAVE.Pay.RequiredArgumentError("WAVE.Pay.Providers.Braintree.initialize()", "init.publicKey");

        provider.clientToken = init.publicKey;
    };
     
    // Payment provider tokenization interface
    // https://developers.braintreepayments.com/guides/credit-cards/client-side/javascript/v2
    provider.tokenize = function (facade, paymentData, callback) {
        if (WAVE.strEmpty(provider.clientToken))
            throw new WAVE.Pay.RequiredArgumentError("WAVE.Pay.Providers.Braintree.tokenize()", "WAVE.Pay.Providers.Braintree.clientToken");

        if (WAVE.strEmpty(paymentData.cardNumber)||
            WAVE.strEmpty(paymentData.cardExpMonth) ||
            WAVE.strEmpty(paymentData.cardExpYear))
            throw new WAVE.Pay.RequiredArgumentError("WAVE.Pay.Providers.Braintree.tokenize()", "paymentData.cardNumber, paymentData.cardExpMonth, paymentData.cardExpYear");

        // prepare braintree.js init card data object
        var brainTreeCardData = 
            {
                number: paymentData.cardNumber, 
                expirationMonth: paymentData.cardExpMonth,
                expirationYear: paymentData.cardExpYear
            };
        if (!WAVE.strEmpty(paymentData.cardCVC))
            brainTreeCardData.cvv = paymentData.cardCVC;
        if (!WAVE.strEmpty(paymentData.cardHolder))
            brainTreeCardData.cardholderName = paymentData.cardHolder;
        if (!WAVE.strEmpty(paymentData.cardAddressZip))
            brainTreeCardData.billingAddress = { postalCode: paymentData.cardAddressZip }; 

        // prepare braintree.js response callback which internally calls outer 'callback' function
        var responseHandler = function (err, nonce) {
            var result = 
                { 
                  requestContext: { "facade": facade, "paymentData": paymentData },
                    OK: true,
                    httpStatus: mapStatus(status),
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

            if (!WAVE.strEmpty(err)) {
                result.OK = false;
                result.errorCode = mapErrorCode(err);
                result.errorType = mapErrorType(err);
                result.errorMessage = err;
                result.errorParams = { };
                result.rawError = err;
            }
            else {
                result.tokenString = nonce;
            }

            callback(result);
        };
        
        // call braintree.js to obtain card payment token
        var clientToken = provider.clientToken;
        provider.clientToken = null; // clear nonce
        var client = new braintree.api.Client({ clientToken: clientToken });
        client.tokenizeCard(brainTreeCardData, responseHandler);
    };

    return provider;

}());