package com.nostradabus.application.parser;

import org.json.JSONException;

import com.nostradabus.application.parser.responses.WebServiceBaseResponse;
import com.nostradabus.connector.parser.JsonParser;

public class WebServiceParser extends JsonParser<WebServiceBaseResponse> {

	@Override
	protected WebServiceBaseResponse parse(Object json) throws JSONException {
		return new WebServiceBaseResponse();
	}

}