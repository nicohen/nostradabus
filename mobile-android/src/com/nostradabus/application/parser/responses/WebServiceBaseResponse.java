package com.nostradabus.application.parser.responses;

import com.nostradabus.connector.parser.ParseResult;

public class WebServiceBaseResponse implements ParseResult {
	private Integer responseCode;
	private String message;
	private Boolean success;
	
	public WebServiceBaseResponse() {}

	public Integer getResponseCode() {
		return responseCode;
	}

	public void setResponseCode(Integer responseCode) {
		this.responseCode = responseCode;
	}

	public String getMessage() {
		return message;
	}

	public void setMessage(String message) {
		this.message = message;
	}

	public Boolean getSuccess() {
		return success;
	}

	public void setSuccess(Boolean success) {
		this.success = success;
	}
}
