

//const apiUrl = process.env.REACT_APP_API_GATEWAY;
const apiUrl = 'http://localhost:8540';
const apiService = {
  getUserData: async (token) => {
    try {
      const response = await fetch(`${apiUrl}/api/userdata`, {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error fetching user data:', error);
      throw error;
    }
  },
   updateUser: async (formData) => {
    try {
      localStorage.removeItem('token');
      console.log("apiurl : ", apiUrl);
      const response = await fetch(`${apiUrl}/api/user/update-user`, {
        method: 'PUT',
        body: formData,
      });

      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error updating user data:', error);
      throw error;
    }
  },
  handleDriversVerification: async (userId) => {
    try {
      // Simuliraj API poziv za prijavu vozača
      console.log("userApiId :", userId);
      const id = {
        userId
       };
      const response = await fetch(`${apiUrl}/api/user/drivers-verification`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(id)
      });
  
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
  
      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error : applying user for driver:', error);
      throw error;
    }
  },
  LogoutButton: async () => {
    
    localStorage.removeItem('token');
    const token = localStorage.getItem('token'); 
    if (!token){
      console.log("remove token - success");
    }
    else  
    {
      console.log('remove token - failed');
    }
     
  },
  GetAllVerifications: async () => {
    try {
      const response = await fetch(`${apiUrl}/api/user/get-verifications`, {
        method: 'GET',
        
      });
  
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
  
      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error: fetching verifications:', error);
      throw error;
    }
  },
  ApproveDriver: async (userId) => {
    try {
      const id = {
        userId
       };
     
      const response = await fetch(`${apiUrl}/api/user/approve-driver`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(id)
      });
  
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
  
      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error: approve driver:', error);
      throw error;
    }
  },
  RejectRequest: async (userId) => {
    try {
      const id = {
        userId
       };
     
      
      const response = await fetch(`${apiUrl}/api/user/reject-drivers-request`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(id)
      });
  
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
  
      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Error: reject driver:', error);
      throw error;
    }
  }
};
export default apiService;
