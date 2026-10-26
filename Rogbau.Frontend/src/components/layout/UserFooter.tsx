import { 
  
  ArrowRightOnRectangleIcon 
} from '@heroicons/react/24/outline';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export const UserFooter: React.FC = () => {

  const {user, logout} = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
  logout();
  navigate("/login");
};
    return(
        <div className="p-4 border-t border-slate-800/80 bg-[#0B1120]">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-full bg-amber-500 text-slate-950 font-bold flex items-center justify-center text-xs">
              {user?.initials}
            </div>
            <div className="text-xs">
              <p className="font-semibold text-slate-200">{user?.fullName}</p>
              <p className="text-slate-500">{user?.role}</p>
            </div>
          </div>
          <button 
            type="button"
            title="Wyloguj"
            className="text-slate-500 hover:text-amber-500 transition-colors p-1 cursor-pointer"
            onClick={handleLogout}
          >
            <ArrowRightOnRectangleIcon className="w-5 h-5" />
          </button>
        </div>
      </div>
    );
}