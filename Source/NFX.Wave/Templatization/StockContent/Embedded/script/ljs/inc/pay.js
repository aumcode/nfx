// Credit Card brands
published.Brands =
{
  VISA: "Visa",
  MASTER_CARD: "MasterCard",
  AMERICAN_EXPRESS: "AmEx",
  DISCOVER: "Discover",
  JCB: "JCB",
  DINERS_CLUB: "Diners Club",
  UNKNOWN: "Card"
};

// Error types
published.ErrorTypes =
{
  OK: 0,                    // Everything work as expected.
  API_CONNECTION_ERROR: 1,  // Failure to connect to Stripe's API.
  API_ERROR: 2,	            // API errors cover any other type of problem (e.g., a temporary problem with Stripe's servers) and are extremely uncommon.
  AUTHENTICATION_ERROR: 3,  // Failure to properly authenticate yourself in the request.
  CARD_ERROR: 4,	          // Card errors are the most common type of error you should expect to handle. They result when the user enters a card that can't be charged for some reason.
  INVALID_REQUEST_ERROR: 5, // Invalid request errors arise when your request has invalid parameters.
  RATE_LIMIT_ERROR: 6,	    // Too many requests hit the API too quickly.
  GENERAL_ERROR: 100
};

// Credit Card errors
published.CardErrors =
{
  OK: 0,                   // Everything work as expected.
  INVALID_NUMBER: 1,	     // The card number is not a valid credit card number.
  INVALID_EXPIRY_MONTH: 2, // The card's expiration month is invalid.
  INVALID_EXPIRY_YEAR: 3,  // The card's expiration year is invalid.
  INVALID_CVC: 4,          // The card's security code is invalid.
  INCORRECT_NUMBER: 5,     // The card number is incorrect.
  EXPIRED_CARD: 6,         // The card has expired.
  INCORRECT_CVC: 7,        // The card's security code is incorrect.
  INCORRECT_ZIP: 8,        // The card's zip code failed validation.
  CARD_DECLINED: 9,        // The card was declined.
  MISSING: 10,             // There is no card on a customer that is being charged.
  PROCESSING_ERROR: 11,    // An error occurred while processing the card.
  GENERAL_ERROR: 100
};

// Pay Terminal Facade
published.Facade = function (providerName, provider, init) {
  if (WAVE.strEmpty(providerName))
    throw new WAVE.Pay.RequiredArgumentError("Facade.ctor()", "providerName");
  if (WAVE.strEmpty(provider))
    throw new WAVE.Pay.RequiredArgumentError("Facade.ctor()", "provider");
  if (WAVE.strEmpty(init))
    throw new WAVE.Pay.RequiredArgumentError("Facade.ctor()", "init");

  var facade = this;
  var fProviderName = providerName;

  // returns pay provider name
  this.providerName = function(){ return fProviderName; };

  WAVE.extend(facade, WAVE.EventManager);

  try {
    // Payment provider specific setup
    provider.initialize(init);
  }
  catch (error) {
    throw new WAVE.Pay.ProviderInitError(error);
  }

  // Payment provider tokenization interface:
  // paymentData - contains CC card data (cc_number, cc_cvc, cc_exp_month, cc_exp_year, cc_holder),
  // callback - a handler function that accepts 'result' object with fields: 'error' and 'token'.
  this.tokenize = function (paymentData, callback) {
    if (WAVE.strEmpty(paymentData))
      throw new WAVE.Pay.RequiredArgumentError("tokenize()", "paymentData");
    if (WAVE.strEmpty(callback))
      throw new WAVE.Pay.RequiredArgumentError("tokenize()", "callback");

    try {
      provider.tokenize(facade, paymentData, callback);
    }
    catch (error) {
      throw new WAVE.Pay.TokenizeError(error);
    }
  };
};

// Errors
published.RequiredArgumentError = function (srcFuncName, argName, inner) {
  this.name = "RequiredArgumentError";
  this.message = srcFuncName + " requires argument '" + argName + "'";
  this.inner = inner;
};
published.RequiredArgumentError.prototype = Error.prototype;

published.ProviderInitError = function (inner) {
  this.name = "ProviderInitError";
  var message = "Critical error during provider setup";
  if (typeof(inner) !== tUNDEFINED && !WAVE.strEmpty(inner.message))
    message += ": " + inner.message;
  this.message = message;
  this.inner = inner;
};
published.ProviderInitError.prototype = Error.prototype;

published.TokenizeError = function (inner) {
  this.name = "TokenizeError";
  var message = "Critical error during tokenization";
  if (typeof(inner) !== tUNDEFINED && !WAVE.strEmpty(inner.message))
    message += ": " + inner.message;
  this.message = message;
  this.inner = inner;
};
published.TokenizeError.prototype = Error.prototype;

published.CardValidationError = function (inner) {
  this.name = "CardValidationError";
  var message = "Card is invalid";
  if (typeof(inner) !== tUNDEFINED && !WAVE.strEmpty(inner.message))
    message += ": " + inner.message;
  this.message = message;
  this.inner = inner;
};
published.CardValidationError.prototype = Error.prototype;

// Utilities
published.validateCardNumber = function (ccNum) {
  if (WAVE.strEmpty(ccNum)) return false;
  ccNum = ccNum.replace(/\s+|-/g, "");
  return ccNum.length >= 10 && ccNum.length <= 16 && published.checkLuhn(ccNum);
};

published.checkLuhn = function (ccNum) {
  if (WAVE.strEmpty(ccNum)) return false;

  var nCheck = 0;
  var nDigit = 0;
  var bEven = false;
  ccNum = ccNum.replace(/\D/g, "");

  for (var n = ccNum.length - 1; n >= 0; n--) {
    var cDigit = ccNum.charAt(n);
    nDigit = parseInt(cDigit, 10);

    if (bEven) {
      if ((nDigit *= 2) > 9) nDigit -= 9;
    }

    nCheck += nDigit;
    bEven = !bEven;
  }

  return (nCheck % 10) === 0;
};

published.validateCVC = function (cvc) {
  if (WAVE.strEmpty(cvc)) return false;
  return cvc.length >= 3 && cvc.length <= 4 && /^\d+$/.test(cvc);
};

published.validateExpirationDate = function (month, year) {
  if (WAVE.strEmpty(month) || WAVE.strEmpty(year)) return false;
  if (year.length === 2) year = "20"+year;

  return /^\d+$/.test(month) &&
          /^\d+$/.test(year) &&
          1 <= month && month <= 12 &&
          year.length === 4 && year > 2015;
};

published.getCardBrand = function (ccNum) {
  if (WAVE.strEmpty(ccNum) || !/^\d{2}/.test(ccNum))
      return published.Brands.UNKNOWN;

  if (/^4/.test(ccNum)) return published.Brands.VISA;
  if (/^5[1-5]/.test(ccNum)) return published.Brands.MASTER_CARD;
  if (/^3[47]/.test(ccNum)) return published.Brands.AMERICAN_EXPRESS;
  if (/^(6011|622(12[6-9]|1[3-9][0-9]|[2-8][0-9]{2}|9[0-1][0-9]|92[0-5]|64[4-9])|65)/.test(ccNum)) return published.Brands.DISCOVER;
  if (/^35(2[89]|[3-8][0-9])/.test(ccNum)) return published.Brands.JCB;
  if (/^3[0689]/.test(ccNum)) return published.Brands.DINERS_CLUB;

  return published.Brands.UNKNOWN;
};