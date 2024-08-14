import LogInPage from './Components/LogInPage';
import RegistrationPage from './Components/RegistrationPage';
import DashboardPage from './Components/DashboardPage';
import UserDataPage from './Components/UserDataPage';
import VerificationPage from './Components/VerificationsPage';
import AdminPage from './Components/AdminPage';
import DriverPage from './Components/DriverPage';
import UserPage from './Components/UserPage';
import PreviousRides from './Components/PreviousRides';
import DriversRides from './Components/DriversRides';
import AllRides from './Components/AllRides';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import './App.css';



function App() {
  const backgroundStyle = {
    backgroundImage: 'url(/Images/taxiApp.jpeg)',
    backgroundSize: 'cover',
    backgroundRepeat: 'no-repeat',
    backgroundPosition: 'center center',
    backgroundAttachment: 'fixed',  
    height: '100vh',
    width: '100%',
    margin: 0,
  };
  return (
    <div className="App">
      
      <Router>
        <div style={backgroundStyle}>
          <Routes>
            <Route path="/" element={<LogInPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/login" element={<LogInPage />} />
            <Route path="/register" element={<RegistrationPage />} />
            <Route path="/user-data" element={<UserDataPage />} />
            <Route path="/verifications" element={<VerificationPage />} />
            <Route path="/rides-history" element={<PreviousRides />} />
            <Route path="/drivers-rides" element={<DriversRides />} />
            <Route path="/all-rides" element={<AllRides />} />
          </Routes>
           
        </div>
    </Router>
    </div>
  );
}

export default App;
