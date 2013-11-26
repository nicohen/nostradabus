/**
* HttpPostWebService.java
*
* @copyright 2011 Monits
* @license   Copyright (C) 2011. All rights reserved
* @version   Release: 1.0.0
* @link      http://www.monits.com/
* @since     1.0.0
*/
package com.nostradabus.connector;

import org.apache.http.client.methods.HttpRequestBase;

import com.nostradabus.connector.parser.JsonParser;
import com.nostradabus.service.ConfigurationService;

public class VendedoresJsonHttpPostWebService extends JsonHttpPostWebService {
	
	public VendedoresJsonHttpPostWebService(JsonParser<?> parser) {
		super(ConfigurationService.DOMAIN_URL, parser);
	}

	/**
	 * @see com.WebService.connector.HttpWebService#makeHttpMethod()
	 */
	@Override
	protected HttpRequestBase makeHttpMethod() {
		
		HttpRequestBase request = super.makeHttpMethod();
		request.addHeader("Content-Type","application/json");

		return request;
	}
}
