import axios from "axios";

const API_BASE_URL = "http://localhost:5178/api"; // Backend base URL

// Example: get all patients
export const getPatients = async () => {
  const response = await axios.get(`${API_BASE_URL}/nurse/patients`);
  return response.data;
};

// Example: register a new patient
export const registerPatient = async (patientData) => {
  const response = await axios.post(`${API_BASE_URL}/nurse/patients`, patientData);
  return response.data;
};

// Example: login
export const login = async (credentials) => {
  const response = await axios.post(`${API_BASE_URL}/auth/login`, credentials);
  return response.data;
};
