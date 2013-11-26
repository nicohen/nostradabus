package com.nostradabus.application.parser.responses;

import java.util.ArrayList;
import java.util.List;

import com.nostradabus.application.model.Bus;

public class BusesResponse extends WebServiceBaseResponse {

	private List<Bus> buses;
	
	public BusesResponse() {
		buses = new ArrayList<Bus>();
	}

	public List<Bus> getBuses() {
		return buses;
	}

	public void setBuses(List<Bus> buses) {
		this.buses = buses;
	}

	public void addBus(Bus bus) {
		this.buses.add(bus);
	}
}
