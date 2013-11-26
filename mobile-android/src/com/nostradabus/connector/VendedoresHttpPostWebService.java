package com.nostradabus.connector;

import org.apache.http.client.methods.HttpRequestBase;

import com.nostradabus.connector.parser.JsonParser;
import com.nostradabus.service.ConfigurationService;

public class VendedoresHttpPostWebService extends JsonHttpPostWebService {
	
	public VendedoresHttpPostWebService(JsonParser<?> parser) {
		super(ConfigurationService.DOMAIN_URL, parser);
	}

	/**
	 * @see com.WebService.connector.HttpWebService#makeHttpMethod()
	 */
	@Override
	protected HttpRequestBase makeHttpMethod() {
		
		HttpRequestBase request = super.makeHttpMethod();
		request.addHeader("Content-Type", "application/json");

		return request;
	}
}
